# RestFlow.DemoServer

RestFlow 클라이언트 라이브러리의 모든 인증 방식을 테스트하기 위한 ASP.NET Core Web API 데모 서버입니다.

## 🎯 개요

이 서버는 다음과 같은 인증 방식을 지원합니다:
- Non-Auth (인증 없음)
- Basic Authentication
- Static Bearer Token
- API Key (Header/Query Parameter)
- OAuth 2.0 (JWT Bearer Token)

## 🚀 실행 방법

### 1. 프로젝트 빌드
```bash
dotnet build
```

### 2. 서버 실행
```bash
dotnet run
```

서버는 기본적으로 다음 URL에서 실행됩니다:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 3. Swagger UI 접속
브라우저에서 다음 URL로 접속하면 API 문서를 확인할 수 있습니다:
```
https://localhost:5001/swagger
```

## 📋 하드코딩된 자격 증명

### Basic Authentication
- **Username**: `admin`
- **Password**: `password`

### Static Bearer Token
- **Token**: `a-static-bearer-token-for-testing`

### API Key
- **Header Name**: `X-API-KEY`
- **Query Parameter Name**: `api_key`
- **Key Value**: `a-static-api-key`

### OAuth 2.0 Client Credentials
- **Client ID**: `restflow-client`
- **Client Secret**: `restflow-secret`
- **Token Endpoint**: `http://localhost:5000/token`
- **Scope**: `read write` (선택 사항)

### OAuth 2.0 Password Credentials
- **Username**: `user`
- **Password**: `pass`
- **Client ID**: `restflow-client`
- **Client Secret**: `restflow-secret`

### OAuth 2.0 Authorization Code
- **Authorization Code**: `static-auth-code-for-testing`
- **Client ID**: `restflow-client`
- **Client Secret**: `restflow-secret`
- **Refresh Token (Password Grant)**: `static-refresh-token-for-password-grant`
- **Refresh Token (Auth Code Grant)**: `static-refresh-token-for-auth-code-grant`

## 🔌 API 엔드포인트

### 1. Health Check
```http
GET /
```
서버 상태를 확인합니다. 인증이 필요 없습니다.

**응답 예시:**
```json
{
  "status": "success",
  "message": "RestFlow.DemoServer is running",
  "version": "1.0",
  "timestamp": "2025-10-30T12:00:00.000Z"
}
```

### 2. Non-Auth Test
```http
GET /api/no-auth/test
```
인증 없이 접근 가능한 테스트 엔드포인트입니다.

### 3. Basic Auth Test
```http
GET /api/basic-auth/test
Authorization: Basic YWRtaW46cGFzc3dvcmQ=
```
Basic Authentication을 테스트합니다.

**cURL 예시:**
```bash
curl -X GET "http://localhost:5000/api/basic-auth/test" \
  -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ="
```

### 4. Bearer Token Test
```http
GET /api/bearer-token/test
Authorization: Bearer a-static-bearer-token-for-testing
```
Static Bearer Token 인증을 테스트합니다.

**cURL 예시:**
```bash
curl -X GET "http://localhost:5000/api/bearer-token/test" \
  -H "Authorization: Bearer a-static-bearer-token-for-testing"
```

### 5. API Key (Header) Test
```http
GET /api/api-key/header-test
X-API-KEY: a-static-api-key
```
API Key (헤더) 인증을 테스트합니다.

**cURL 예시:**
```bash
curl -X GET "http://localhost:5000/api/api-key/header-test" \
  -H "X-API-KEY: a-static-api-key"
```

### 6. API Key (Query) Test
```http
GET /api/api-key/query-test?api_key=a-static-api-key
```
API Key (쿼리 파라미터) 인증을 테스트합니다.

**cURL 예시:**
```bash
curl -X GET "http://localhost:5000/api/api-key/query-test?api_key=a-static-api-key"
```

### 7. OAuth 2.0 Protected Resource
```http
GET /api/oauth/protected
Authorization: Bearer <JWT_TOKEN>
```
OAuth 2.0 JWT 토큰으로 보호된 리소스에 접근합니다.

## 🔑 OAuth 2.0 토큰 발급

### Token Endpoint
```http
POST /token
Content-Type: application/x-www-form-urlencoded
```

### Grant Type: client_credentials

**요청:**
```bash
curl -X POST "http://localhost:5000/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=restflow-client&client_secret=restflow-secret&scope=read write"
```

**응답:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 120,
  "scope": "read write"
}
```

### Grant Type: password

**요청:**
```bash
curl -X POST "http://localhost:5000/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=user&password=pass&client_id=restflow-client&client_secret=restflow-secret"
```

**응답:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 120,
  "refresh_token": "static-refresh-token-for-password-grant",
  "scope": "read write"
}
```

### Grant Type: authorization_code

**요청:**
```bash
curl -X POST "http://localhost:5000/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code&code=static-auth-code-for-testing&client_id=restflow-client&client_secret=restflow-secret&redirect_uri=http://localhost"
```

**응답:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 120,
  "refresh_token": "static-refresh-token-for-auth-code-grant",
  "scope": "read write"
}
```

### Grant Type: refresh_token

**요청:**
```bash
curl -X POST "http://localhost:5000/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=static-refresh-token-for-password-grant&client_id=restflow-client&client_secret=restflow-secret"
```

**응답:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 120,
  "refresh_token": "static-refresh-token-for-password-grant",
  "scope": "read write"
}
```

## 🧪 RestFlow 클라이언트 테스트 예시

### 1. Non-Auth
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000");

var result = await client.GetAsync<dynamic>("/api/no-auth/test");
```

### 2. Basic Auth
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithBasicAuth("admin", "password");

var result = await client.GetAsync<dynamic>("/api/basic-auth/test");
```

### 3. Bearer Token
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithBearerToken("a-static-bearer-token-for-testing");

var result = await client.GetAsync<dynamic>("/api/bearer-token/test");
```

### 4. API Key (Header)
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithApiKey("X-API-KEY", "a-static-api-key", ApiKeyLocation.Header);

var result = await client.GetAsync<dynamic>("/api/api-key/header-test");
```

### 5. API Key (Query)
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithApiKey("api_key", "a-static-api-key", ApiKeyLocation.QueryParam);

var result = await client.GetAsync<dynamic>("/api/api-key/query-test");
```

### 6. OAuth 2.0 Client Credentials
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithOAuthClientCredentials(
        tokenEndpoint: "http://localhost:5000/token",
        clientId: "restflow-client",
        clientSecret: "restflow-secret",
        options: new OAuthOptions
        {
            ClockSkewSeconds = 30,
            EnableAutoRetryOn401 = true
        });

var result = await client.GetAsync<dynamic>("/api/oauth/protected");
```

### 7. OAuth 2.0 Password Credentials
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithOAuthPasswordCredentials(
        tokenEndpoint: "http://localhost:5000/token",
        username: "user",
        password: "pass",
        clientId: "restflow-client",
        clientSecret: "restflow-secret");

var result = await client.GetAsync<dynamic>("/api/oauth/protected");
```

### 8. OAuth 2.0 Authorization Code (Refresh Token Flow)
```csharp
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5000")
    .WithOAuthAuthorizationCode(
        tokenEndpoint: "http://localhost:5000/token",
        refreshToken: "static-refresh-token-for-auth-code-grant",
        clientId: "restflow-client",
        clientSecret: "restflow-secret");

var result = await client.GetAsync<dynamic>("/api/oauth/protected");
```

## ⚙️ 주요 설정

### JWT 토큰 만료 시간
- **기본값**: 120초 (2분)
- **목적**: 토큰 갱신 및 Clock Skew 테스트

### CORS 정책
- **모든 Origin 허용**: 테스트를 위해 모든 도메인에서 접근 가능
- **모든 HTTP 메서드 허용**: GET, POST, PUT, DELETE, PATCH 등
- **모든 헤더 허용**: 커스텀 헤더 사용 가능

### 인증 스킴
서버는 여러 인증 스킴을 동시에 지원하며, 요청 경로에 따라 자동으로 적절한 스킴을 선택합니다.

## 📝 참고사항

1. **데이터베이스 없음**: 모든 자격 증명은 코드에 하드코딩되어 있습니다.
2. **테스트 전용**: 프로덕션 환경에서 사용하지 마세요.
3. **짧은 토큰 수명**: JWT 토큰은 2분 후 만료되어 갱신 로직을 테스트할 수 있습니다.
4. **Clock Skew 없음**: JWT 검증 시 Clock Skew를 0으로 설정하여 정확한 만료 시간 테스트가 가능합니다.

## 🐛 문제 해결

### 401 Unauthorized 오류
- 자격 증명이 정확한지 확인하세요.
- OAuth 2.0의 경우 토큰이 만료되지 않았는지 확인하세요.

### CORS 오류
- 서버가 CORS를 허용하도록 설정되어 있으므로, 클라이언트 측 설정을 확인하세요.

### 토큰 만료 오류
- 토큰은 2분 후 만료됩니다. 새 토큰을 발급받으세요.
- RestFlow 클라이언트는 자동으로 토큰을 갱신합니다.

## 📄 라이선스

이 프로젝트는 RestFlow 라이브러리의 테스트 목적으로만 사용됩니다.
