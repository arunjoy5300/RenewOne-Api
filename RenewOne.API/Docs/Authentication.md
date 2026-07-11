# Authentication

---

## Login

### Endpoint

POST /api/Auth/login

### Description

Authenticates a user and returns a JWT token.

### Authorization

❌ Not Required

### Request

```json
{
  "userName": "anand",
  "password": "password"
}
```

### Success Response

```json
{
  "isSuccess": true,
  "data": {
    "userId": "793b3b3e-9fed-4724-bfd2-cd80d997d78a",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9",
    "fullName": "Jon Doe",
    "userType": "Agent"
  },
  "message": "Login Success",
  "statusCode": 200,
  "errors": null
}
```

### Error Codes

| Code | Description |
|------|-------------|
|400|Validation Failed|
|401|Invalid Credentials|
|500|Internal Server Error|

### Business Rules

- User must be active.
- Email and password are required.
- JWT expires in X hours.