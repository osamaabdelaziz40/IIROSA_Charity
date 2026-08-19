## Deferred from: code review of 3-1-list-all-charities and 3-2-verify-charity-name-availability (2026-08-19)

- **Tenancy claims have no revocation path.** Moving a user between charities has no effect until
  their access token expires (30 minutes). Inherent to stateless JWT; would need a token-version
  claim or a server-side revocation list.
- **No tests for a security-boundary change.** `ApplyCallerScope` has four distinct branches
  (charity claim, country claim, neither, unauthenticated) and zero tests. `Backend/tests/` holds no
  test project. Deferred by explicit user decision on 2026-08-19.
- **Validation and normalisation sit in the controller** rather than a FluentValidation validator in
  the service layer. The charity validator is scoped to stories 3-3 and 3-5.
- **Server-side response strings are hard-coded English.** Applies to every controller in the
  project, not just the new endpoint.
- **`CharitiesController` inherits `ControllerBase`, not `ApiController`.** Pre-existing in 20 of 21
  controllers; fixing one in isolation makes it inconsistent with its neighbours.
- **`text-input` component breaks the mandated 4-file shape** — `.css` rather than `.scss`, and no
  `.spec.ts`, so the new `nameTaken` branch ships with no coverage.
- **`catchError(this.handleError)` is passed unbound** in `charity.service.ts`. `handleError` is a
  plain private method used this way by roughly 25 methods; either fine everywhere or broken
  everywhere.
- **`ngx-bootstrap@^12` is incompatible with Angular 18.** `npm install` fails with `ERESOLVE`;
  every developer and CI job currently needs `--legacy-peer-deps`. Needs an upgrade to v18.

### Deferred by decision during the same review

- **`ApiResponse<T>` envelope is not used by `CharitiesController`.** CLAUDE.md requires every
  endpoint to return `Framework.Core.ApiResponse`/`ApiResponse<T>`, but only 1 of 21 controllers
  does. Converting a single endpoint would make its own controller internally inconsistent and
  break any client that reads its neighbours. This wants one migration across all controllers,
  scoped as its own story, not a per-story fix.

- **No unique index on `Charity.Name`.** `CharityConfiguration` has `HasIndex(x => x.Name)` without
  `.IsUnique()`, unlike `Code`. Two concurrent creates of the same name both pass
  `IsNameUniqueAsync` and both insert. Adding the constraint needs a data check first, because the
  migration fails if duplicates already exist. Find them with:

  ```sql
  SELECT LTRIM(RTRIM(Name)) AS Name, COUNT(*) AS Copies
  FROM [IIROSA].[Charity]
  WHERE IsDeleted = 0
  GROUP BY LTRIM(RTRIM(Name))
  HAVING COUNT(*) > 1;
  ```

  If that returns nothing, the index can be added safely. Note the application now normalises
  names on save, so new leading/trailing-space duplicates can no longer be created.
