import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { Observable, Subject, take } from 'rxjs';
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

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private notificationSubject = new Subject<NotificationMessage>();
  private connectionAttempted = false;
  private isSignalREnabled = true;

  public notifications$ = this.notificationSubject.asObservable();

  constructor(private authService: AuthService) { }

  startConnection(): void {
    if (this.connectionAttempted || !this.isSignalREnabled) {
      return;
    }

    this.connectionAttempted = true;

    const hubUrl = `${environment.apiUrl || 'https://localhost:60960'}/hubs/notifications`;

    const connectionBuilder = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        skipNegotiation: false,
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
      .configureLogging(signalR.LogLevel.Warning);

    // Add JWT token if user is authenticated
    if (this.authService.isAuthenticated()) {
      const token = this.authService.getAccessToken();
      connectionBuilder.withUrl(hubUrl, {
        accessTokenFactory: () => token || '',
        skipNegotiation: false,
        withCredentials: true
      });
    }

    this.hubConnection = connectionBuilder.build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection started successfully');
        this.registerHandlers();
      })
      .catch(err => {
        console.warn('SignalR connection failed:', err.message);
        this.isSignalREnabled = false;
      });
  }

  private registerHandlers(): void {
    if (!this.hubConnection) {
      return;
    }

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
    });

    this.hubConnection.onreconnecting(error => {
      console.log('SignalR reconnecting...', error);
    });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionAttempted = false;
    }
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.hubConnection?.state ?? signalR.HubConnectionState.Disconnected;
  }

  isConnected(): boolean {
    return this.getConnectionState() === signalR.HubConnectionState.Connected;
  }
}
