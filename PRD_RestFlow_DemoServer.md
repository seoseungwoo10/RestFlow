## 📋 RestFlow.DemoServer PRD (제품 요구 사항 문서) - v1.1

### 1\. 프로젝트 개요

  * **프로젝트명:** `RestFlow.DemoServer`
  * **목표:** `RestFlow` C\# 클라이언트 라이브러리의 모든 인증 방식(Non-Auth, Basic, Bearer, API Key, OAuth 2.0)을 테스트하기 위한 **단순화된 단일 ASP.NET Core Web API 서버**를 제공합니다.
  * **핵심 원칙:**
      * **데이터베이스 없음:** 모든 자격 증명(ID/PW, Client ID, API Key)은 코드 내에 하드코딩(Hardcoded)합니다.
      * **간단한 로직:** 엔드포인트는 인증을 검증하고, 성공/실패 여부만 단순하게 반환합니다.
      * **단일 프로젝트:** 하나의 서버 실행으로 모든 인증 시나리오 테스트를 지원합니다.

### 2\. 기술 사양

  * **프로젝트 타입:** ASP.NET Core Web API
  * **대상 프레임워크:** `net8.0` (또는 `net6.0` 이상)
  * **언어:** C\#
  * **개발 환경:** Visual Studio 2022

### 3\. 핵심 요구사항

  * **CORS(Cross-Origin Resource Sharing) 지원:**
      * `RestFlow`의 테스트용 WPF 앱 또는 웹 애플리케이션에서 API를 호출할 수 있도록, \*\*모든 오리진(AnyOrigin), 모든 메서드(AnyMethod), 모든 헤더(AnyHeader)\*\*를 허용하는 CORS 정책을 활성화해야 합니다.
  * **다중 인증 스킴(Scheme) 등록:**
      * `Program.cs` (또는 `Startup.cs`)에 Basic, Static Bearer, API Key, JWT Bearer 인증 핸들러를 **동시에 등록**해야 합니다.
      * 각 엔드포인트는 `[Authorize(AuthenticationSchemes = "...")]` 어트리뷰트를 사용하여 특정 인증 방식만 허용하도록 명시적으로 지정해야 합니다.

### 4\. API 엔드포인트 명세

서버는 다음의 엔드포인트를 제공해야 합니다. 모든 성공 응답은 간단한 JSON 객체(예: `{ "status": "success", "authType": "..." }`)를 반환합니다.

| 경로 (Path) | 메서드 | 인증 방식 | 설명 |
| :--- | :--- | :--- | :--- |
| `/` | `GET` | **Non-Auth** | 서버가 실행 중인지 확인하는 간단한 상태 확인용 엔드포인트입니다. |
| `/api/no-auth/test` | `GET` | **Non-Auth** | `RestFlow`의 `Non Auth` 테스트를 위한 공개 엔드포인트입니다. |
| `/api/basic-auth/test` | `GET` | **Basic Auth** | `Authorization: Basic <base64(ID:PW)>` 헤더를 검증합니다.<br>\* (하드코딩 예: `admin` / `password`) |
| `/api/bearer-token/test` | `GET` | **Static Bearer** | `Authorization: Bearer <token>` 헤더를 검증합니다.<br>\* (하드코딩 예: `a-static-bearer-token-for-testing`) |
| `/api/api-key/header-test` | `GET` | **API Key (Header)** | `X-API-KEY: <key>`와 같은 특정 헤더의 API 키를 검증합니다.<br>\* (하드코딩 예: `a-static-api-key`) |
| `/api/api-key/query-test` | `GET` | **API Key (Query)** | `?api_key=<key>`와 같은 쿼리 파라미터의 API 키를 검증합니다. |
| `/api/oauth/protected` | `GET` | **OAuth 2.0 (JWT)** | `/token` 엔드포인트에서 발급된 **유효한 JWT**가 있는지 검증합니다. |

-----

### 5\. OAuth 2.0 토큰 발급 엔드포인트 (`/token`)

`RestFlow` 클라이언트의 고급 OAuth 2.0 기능(토큰 갱신, 401 재시도, Clock Skew)을 테스트하려면, 서버는 **매우 짧은 수명(Short-lived)의 JWT**를 발급해야 합니다.

  * **경로:** `/token`
  * **메서드:** `POST`
  * **Content-Type:** `application/x-www-form-urlencoded`

이 엔드포인트는 `grant_type` 파라미터에 따라 분기 처리되어야 합니다.

#### 5.1. Grant Type: `client_credentials`

  * **용도:** `RestFlow`의 `WithOAuthClientCredentials` 테스트용입니다.
  * **요청 파라미터:**
      * `grant_type=client_credentials`
      * `client_id=<id>`
      * `client_secret=<secret>`
      * `scope=<scope>` (선택 사항)
  * **서버 로직 (하드코딩):**
      * `client_id`가 `restflow-client`이고 `client_secret`가 `restflow-secret`인지 확인합니다.
      * 성공 시, **수명이 매우 짧은(예: 60\~120초)** JWT `access_token`을 생성하여 발급합니다.
  * **성공 응답 (JSON):**
    ```json
    {
        "access_token": "eyJhbGciOiJIUzI1Ni...",
        "token_type": "Bearer",
        "expires_in": 120,
        "scope": "read write"
    }
    ```

#### 5.2. Grant Type: `password` (Resource Owner) 

  * **용도:** `RestFlow`의 `WithOAuthPasswordCredentials` 테스트용입니다.
  * **요청 파라미터:**
      * `grant_type=password`
      * `username=<user>`
      * `password=<pass>`
      * `client_id=<id>`
      * `client_secret=<secret>`
      * `scope=<scope>` (선택 사항)
  * **서버 로직 (하드코딩):**
      * `username`이 `user`이고 `password`가 `pass`인지 확인합니다.
      * **(추가)** `client_id`가 `restflow-client`이고 `client_secret`가 `restflow-secret`인지 **함께** 확인합니다.
      * 성공 시, 짧은 수명의 JWT `access_token`과 \*\*정적(Static)인 `refresh_token`\*\*을 발급합니다.
  * **성공 응답 (JSON):** **[3번 수정사항 반영]**
    ```json
    {
        "access_token": "eyJhbGciOiJIZ...",
        "token_type": "Bearer",
        "expires_in": 120,
        "refresh_token": "static-refresh-token-for-password-grant",
        "scope": "read write"
    }
    ```

#### 5.3. Grant Type: `authorization_code` 

  * **용도:** `RestFlow`의 `WithOAuthAuthorizationCode` (초기 토큰 획득) 테스트용입니다.
  * **참고:** 실제 브라우저 리디렉션은 구현하지 않습니다. 클라이언트가 \*\*하드코딩된 `code`\*\*를 가졌다고 가정하고 테스트합니다.
  * **요청 파라미터:**
      * `grant_type=authorization_code`
      * `code=<code>`
      * `redirect_uri=<uri>` (필수지만, 서버는 검증하지 않음)
      * `client_id=<id>`
      * `client_secret=<secret>`
  * **서버 로직 (하드코딩):**
      * `code`가 `static-auth-code-for-testing`인지 확인합니다.
      * `client_id`가 `restflow-client`이고 `client_secret`가 `restflow-secret`인지 확인합니다.
      * 성공 시, 짧은 수명의 JWT `access_token`과 \*\*정적(Static)인 `refresh_token`\*\*을 발급합니다.
  * **성공 응답 (JSON):** **[3번 수정사항 반영]**
    ```json
    {
        "access_token": "eyJhbGciOiJIZ...",
        "token_type": "Bearer",
        "expires_in": 120,
        "refresh_token": "static-refresh-token-for-auth-code-grant",
        "scope": "read write"
    }
    ```

#### 5.4. Grant Type: `refresh_token` (기존 5.3)

  * **용도:** `RestFlow`의 `WithOAuthAuthorizationCode` (토큰 갱신) 및 `password` 플로우의 갱신 테스트용입니다.
  * **요청 파라미터:**
      * `grant_type=refresh_token`
      * `refresh_token=<token>`
      * `client_id=<id>` (선택 사항이지만, 검증 권장)
      * `client_secret=<secret>` (선택 사항이지만, 검증 권장)
  * **서버 로직 (하드코딩):**
      * `refresh_token`이 위에서 발급한 정적 리프레시 토큰(예: `static-refresh-token-for-password-grant` 또는 `static-refresh-token-for-auth-code-grant`) 중 하나와 일치하는지 확인합니다.
      * 성공 시, **새로운** 짧은 수명의 JWT `access_token`을 발급합니다. (보안을 위해 새 Refresh Token을 발급할 수도 있으나, 데모 서버에서는 기존 Refresh Token을 계속 사용하도록 허용)
  * **성공 응답 (JSON):** **[3번 수정사항 반영]**
    ```json
    {
        "access_token": "eyJhbGciOiJIZ...new...",
        "token_type": "Bearer",
        "expires_in": 120,
        "refresh_token": "static-refresh-token-for-password-grant",
        "scope": "read write"
    }
    ```

### 6\. 주요 산출물

1.  **`RestFlow.DemoServer` 프로젝트:** 단일 ASP.NET Core Web API 프로젝트.
2.  **`README.md`:** 서버 실행 방법 및 테스트에 필요한 모든 하드코딩된 자격 증명(ID/PW, API Key, Client ID, Static Code 등)을 명시한 간단한 문서.