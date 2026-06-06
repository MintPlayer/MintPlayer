import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, from, throwError, of } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { BaseUrlService } from '@mintplayer/ng-base-url';
import { MINTPLAYER_API_VERSION, LoginResult } from '@mintplayer/ng-client';
import { WebAuthnCredentialInfo } from '../../interfaces/webauthn-credential-info';

@Injectable({
  providedIn: 'root'
})
export class WebAuthnService {
  private baseUrl: string;

  constructor(
    private http: HttpClient,
    private baseUrlService: BaseUrlService,
    @Inject(MINTPLAYER_API_VERSION) private apiVersion: string,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.baseUrl = this.baseUrlService.getBaseUrl();
  }

  /**
   * Check if WebAuthn is supported in the current browser
   */
  isSupported(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    return !!window.PublicKeyCredential;
  }

  /**
   * Check if conditional mediation (autofill) is available
   */
  isConditionalMediationAvailable(): Observable<boolean> {
    if (!this.isSupported()) {
      return of(false);
    }
    const pkc = PublicKeyCredential as any;
    if (typeof pkc.isConditionalMediationAvailable !== 'function') {
      return of(false);
    }
    return from(pkc.isConditionalMediationAvailable() as Promise<boolean>);
  }

  /**
   * Register a new passkey for the current user
   */
  registerPasskey(displayName: string): Observable<WebAuthnCredentialInfo> {
    if (!this.isSupported()) {
      return throwError(() => new Error('WebAuthn is not supported in this browser'));
    }

    // Step 1: Get registration options from server
    return this.http.post<PublicKeyCredentialCreationOptions>(
      `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/register/options`,
      { displayName }
    ).pipe(
      switchMap(options => {
        // Convert base64url strings to ArrayBuffers
        const publicKeyOptions = this.convertCreationOptions(options);

        // Step 2: Create credential using browser API
        return from(navigator.credentials.create({ publicKey: publicKeyOptions }) as Promise<PublicKeyCredential>);
      }),
      switchMap(credential => {
        if (!credential) {
          return throwError(() => new Error('Credential creation was cancelled'));
        }

        // Step 3: Send attestation response to server
        const attestationResponse = credential.response as AuthenticatorAttestationResponse;
        const attestationResponseAny = attestationResponse as any;
        const body = {
          id: this.bufferToBase64Url(credential.rawId),
          rawId: this.bufferToBase64Url(credential.rawId),
          attestationObject: this.bufferToBase64Url(attestationResponse.attestationObject),
          clientDataJSON: this.bufferToBase64Url(attestationResponse.clientDataJSON),
          transports: attestationResponseAny.getTransports ? attestationResponseAny.getTransports() : [],
          displayName
        };

        return this.http.post<WebAuthnCredentialInfo>(
          `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/register/complete`,
          body
        );
      })
    );
  }

  /**
   * Login with a passkey (no email required for discoverable credentials)
   */
  loginWithPasskey(email?: string): Observable<LoginResult> {
    if (!this.isSupported()) {
      return throwError(() => new Error('WebAuthn is not supported in this browser'));
    }

    // Step 1: Get assertion options from server
    return this.http.post<PublicKeyCredentialRequestOptions>(
      `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/login/options`,
      { email: email || '' }
    ).pipe(
      switchMap(options => {
        // Convert base64url strings to ArrayBuffers
        const publicKeyOptions = this.convertRequestOptions(options);

        // Step 2: Get credential using browser API
        return from(navigator.credentials.get({ publicKey: publicKeyOptions }) as Promise<PublicKeyCredential>);
      }),
      switchMap(credential => {
        if (!credential) {
          return throwError(() => new Error('Credential retrieval was cancelled'));
        }

        // Step 3: Send assertion response to server
        const assertionResponse = credential.response as AuthenticatorAssertionResponse;
        const body = {
          id: this.bufferToBase64Url(credential.rawId),
          rawId: this.bufferToBase64Url(credential.rawId),
          clientDataJSON: this.bufferToBase64Url(assertionResponse.clientDataJSON),
          authenticatorData: this.bufferToBase64Url(assertionResponse.authenticatorData),
          signature: this.bufferToBase64Url(assertionResponse.signature),
          userHandle: assertionResponse.userHandle ? this.bufferToBase64Url(assertionResponse.userHandle) : null
        };

        return this.http.post<LoginResult>(
          `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/login`,
          body
        );
      })
    );
  }

  /**
   * Get all registered passkeys for the current user
   */
  getCredentials(): Observable<WebAuthnCredentialInfo[]> {
    return this.http.get<WebAuthnCredentialInfo[]>(
      `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/credentials`
    );
  }

  /**
   * Remove a passkey
   */
  removeCredential(id: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/web/${this.apiVersion}/Account/webauthn/credentials/${id}`
    );
  }

  /**
   * Convert server options to WebAuthn API format
   */
  private convertCreationOptions(options: any): PublicKeyCredentialCreationOptions {
    return {
      ...options,
      challenge: this.base64UrlToBuffer(options.challenge),
      user: {
        ...options.user,
        id: this.base64UrlToBuffer(options.user.id)
      },
      excludeCredentials: options.excludeCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64UrlToBuffer(cred.id)
      })) || []
    };
  }

  private convertRequestOptions(options: any): PublicKeyCredentialRequestOptions {
    return {
      ...options,
      challenge: this.base64UrlToBuffer(options.challenge),
      allowCredentials: options.allowCredentials?.map((cred: any) => ({
        ...cred,
        id: this.base64UrlToBuffer(cred.id)
      })) || []
    };
  }

  /**
   * Base64URL to ArrayBuffer
   */
  private base64UrlToBuffer(base64url: string): ArrayBuffer {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padLen = (4 - base64.length % 4) % 4;
    const padded = base64 + '='.repeat(padLen);
    const binary = atob(padded);
    const buffer = new ArrayBuffer(binary.length);
    const view = new Uint8Array(buffer);
    for (let i = 0; i < binary.length; i++) {
      view[i] = binary.charCodeAt(i);
    }
    return buffer;
  }

  /**
   * ArrayBuffer to Base64URL
   */
  private bufferToBase64Url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.length; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    const base64 = btoa(binary);
    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  }
}
