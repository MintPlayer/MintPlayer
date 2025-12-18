export interface WebAuthnCredentialInfo {
  id: number;
  displayName: string;
  regDate: Date;
  lastUsed: Date | null;
}
