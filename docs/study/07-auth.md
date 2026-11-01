# Authentication and Roles

ASP.NET Core Identity hashes passwords using platform-provided password hashing; the application never reads or stores plaintext passwords. Registration assigns the `User` role. Startup ensures `User` and `Admin` roles exist. Protected routes use `[Authorize]`, retrieve the Identity `NameIdentifier` claim, and pass that ID to every query/command. Database predicates enforce ownership again rather than trusting only the UI.
