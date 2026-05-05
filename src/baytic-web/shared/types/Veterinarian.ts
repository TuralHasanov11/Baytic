export type VeterinarianVerificationStatus = 'Pending' | 'Verified' | 'Suspended'

export interface VeterinarianCredential {
  credentialId: string
  credentialName: string
  issuer: string
  issuedOn: string
  expiresOn?: string | null
  isVerified: boolean
  verifiedBy?: string | null
  verifiedAtUtc?: string | null
}

export interface VeterinarianProfileLink {
  linkId: string
  label: string
  url: string
}

export interface VeterinarianProfile {
  id: string
  userId: string
  displayName: string
  biography?: string | null
  location: string
  yearsOfExperience: number
  verificationStatus: VeterinarianVerificationStatus
  isFeatured: boolean
  credentials: VeterinarianCredential[]
  expertiseTags: string[]
  profileLinks: VeterinarianProfileLink[]
}
