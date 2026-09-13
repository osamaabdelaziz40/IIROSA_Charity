import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export interface NotificationMessage {
  id: string;
  title: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  timestamp: Date;
}

export interface SignalRNotification {
  id: string;
  title: string;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  timestamp: string;
}

/**
 * Live-notification hub connection (UC-NTF delivery).
 *
 * Lifecycle rules — each exists because of a past push-loss incident:
 *  - the access token is read through a factory so every (re)connect sends the
 *    CURRENT token; a token captured as a constant went stale after a refresh
 *    or re-login and silently dropped the connection out of the hub's user map;
 *  - a failed start is RETRYABLE with backoff — a single early failure used to
 *    disable the service for the whole session (connectionAttempted latched),
 *    so every later push to this user vanished until a hard reload;
 *  - 'ReceiveNotification' is registered BEFORE start() resolves, so a push
 *    landing in the start-completion gap cannot be missed;
 *  - withAutomaticReconnect rides out transient drops; onclose (reconnect gave
 *    up) falls back to the same retry ladder.
 */
@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationSubject = new Subject<NotificationMessage>();
  private retryTimer: ReturnType<typeof setTimeout> | null = null;
  private retryCount = 0;
  /** True only while stopConnection tears the hub down — keeps onclose from retrying. */
  private manualStop = false;

  public notifications$ = this.notificationSubject.asObservable();

  constructor(private authService: AuthService) { }

  startConnection(): void {
    this.manualStop = false;
    if (this.hubConnection) {
      return; // already connected or a start is in flight
    }

    const hubUrl = `${environment.apiUrl || 'https://localhost:60960'}/hubs/notifications`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        // Factory, not a captured token: re-authenticates on every reconnect.
        accessTokenFactory: () => this.authService.getAccessToken() || '',
        withCredentials: true
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryContext => {
          if (retryContext.previousRetryCount === 0) {
            return 2000;
          } else if (retryContext.previousRetryCount < 3) {
            return 10000;
          } else {
            return 30000;
          }
        }
      })
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerHandlers();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection started successfully');
        this.retryCount = 0;
      })
      .catch(err => {
        console.warn('SignalR connection failed:', err.message);
        this.hubConnection = null;
        this.scheduleRetry();
      });
  }

  private registerHandlers(): void {
    if (!this.hubConnection) {
      return;
    }

    // Registered before start() completes — an early push must find a listener.
    this.hubConnection.on('ReceiveNotification', (notification: SignalRNotification) => {
      const message: NotificationMessage = {
        id: notification.id,
        title: notification.title,
        message: notification.message,
        type: notification.type as 'success' | 'error' | 'warning' | 'info' || 'info',
        timestamp: new Date(notification.timestamp)
      };
      this.notificationSubject.next(message);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
    });

    this.hubConnection.onclose(() => {
      console.log('SignalR connection closed');
      this.hubConnection = null;
      if (!this.manualStop) {
        this.scheduleRetry();
      }
    });

    this.hubConnection.onreconnecting(error => {
      console.log('SignalR reconnecting...', error);
    });
  }

  /** Failed starts retry with backoff: 5s, 15s, 30s, then every 60s. */
  private scheduleRetry(): void {
    if (this.retryTimer !== null) {
      return;
    }

    const delay = this.retryCount === 0 ? 5000
      : this.retryCount === 1 ? 15000
        : this.retryCount === 2 ? 30000
          : 60000;
    this.retryCount++;

    this.retryTimer = setTimeout(() => {
      this.retryTimer = null;
      this.startConnection();
    }, delay);
  }

  stopConnection(): void {
    if (this.retryTimer !== null) {
      clearTimeout(this.retryTimer);
      this.retryTimer = null;
    }
    this.retryCount = 0;

    if (this.hubConnection) {
      this.manualStop = true;
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.hubConnection?.state ?? signalR.HubConnectionState.Disconnected;
  }

  isConnected(): boolean {
    return this.getConnectionState() === signalR.HubConnectionState.Connected;
  }
}
