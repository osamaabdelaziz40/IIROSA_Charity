import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

/**
 * UC-SYS-13 — the platform's last-resort surface (`#/error`). The interceptor lands here
 * when a request failed in a way no screen owns: a middleware-signed 5xx (body carried a
 * traceId) or a network-level failure (status 0). The page is intentionally OUTSIDE the
 * auth-gated layout — an unauthenticated session must still get the friendly page, not a
 * blank screen or a login loop. State (traceId / status) rides the router's history state,
 * which survives a refresh of the error URL itself.
 */
@Component({
  selector: 'app-error-page',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule],
  templateUrl: './error-page.component.html',
  styleUrls: ['./error-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorPageComponent implements OnInit {
  /** Support-reference id from the middleware's error body (null on network failures). */
  traceId: string | null = null;
  /** HTTP status the interceptor was told about (0 = network-level failure). */
  status: number | null = null;

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Angular merges router navigation `state` into the browser's history.state — reading
    // it directly keeps the value across a refresh of #/error.
    const state = history.state as { traceId?: string | null; status?: number | null } | null;
    this.traceId = state?.traceId ?? null;
    this.status = state?.status ?? null;
  }

  backToDashboard(): void {
    void this.router.navigate(['/dashboard']);
  }
}
