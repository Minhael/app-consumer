# Privilege Authorizer

This authorizer is inspried by [Role-based access control](https://en.wikipedia.org/wiki/Role-based_access_control).

# Privilege format

```
role/value

For example,
company/000
vendor/123456
```

# Principle

`ICredentialProvider` produces list of privileges (Roles) based the user claims or other means of authentication results (Subjects). Multiple `IPermissionResolver` consume list of privileges and resolve to list of permissions.

Privileges have its own priority. Generally, high priority privileges settings will overwrite permission values from low priority privileges. `IPermissionResolver` have the final right to change this behaviour.
