export const environment = {
  production: true,
  // Review P24 2026-08-26: the trailing /api is GONE — every service in the app builds
  // `${environment.apiUrl}/api/<Controller>`; the suffix doubled the prefix to
  // /api/api/... in production builds and every call 404'd. Keep this host WITHOUT the
  // /api path; the reverse proxy maps the host root to the API origin.
  apiUrl: 'https://api.iirosa.org'
};
