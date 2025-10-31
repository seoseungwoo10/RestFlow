
 ## 📋 C\# RestFlow 라이브러리 및 테스트 프로그램 PRD

다음은 Microsoft Visual Studio 2022 Professional 환경에서 C\#으로 개발할 `RestClient` 라이브러리 및 API 테스트 프로그램에 대한 제품 요구 사항 문서(PRD)입니다.

-----

### 1\. 프로젝트 개요

  * **솔루션명:** `RestFlow`
  * **프로젝트명:** `RestFlow.Client10`, `RestFlow.Client20`
  * **목표:** .NET Standard 및 .NET Framework의 다양한 버전을 지원하는 재사용 가능하고 확장 가능한 C# HTTP 클라이언트 라이브러리를 개발합니다. 이 라이브러리는 최신 RESTful API와 통신 시 필요한 복잡한 인증 로직을 추상화하여 개발자가 비즈니스 로직에 집중할 수 있도록 돕습니다.
  * **핵심 문제:**
      * 프로젝트마다 `HttpClient` 설정 및 인증 헤더 관리를 반복적으로 구현해야 하는 비효율성.
      * 다양한 .NET 프레임워크 환경(.NET Framework, .NET Core, .NET 5+)에서 일관된 API 호출 방식을 유지하기 어려움.
      * OAuth 2.0과 같은 복잡한 인증 흐름을 수동으로 관리할 때 발생하는 오류 가능성.
  * **솔루션:**
      * 주요 인증 방식(OAuth 2.0, Basic Auth 등)을 내장 지원하는 Fluent API 스타일의 클라이언트 라이브러리.
      * **두 개의 프로젝트로 분리**하여 구형 및 신형 프레임워크를 각각 지원하는 NuGet 패키지 제공.
      * 라이브러리의 모든 기능을 검증하는 포괄적인 단위/통합 테스트 스위트.

-----

### 2\. 주요 기능 및 요구사항 (라이브러리)

#### 2.1. 핵심 HTTP 클라이언트 기능

  * `System.Net.Http.HttpClient`를 기반으로 래핑(Wrapping).
  * **CRUD 메서드 지원:** `GET`, `POST`, `PUT`, `DELETE`, `PATCH` 지원.
  * **직렬화/역직렬화:**
      * JSON (Request/Response) 자동 (de)serialization.
      * `Newtonsoft.Json` 을 지원.
  * **요청 관리:**
      * Request/Response 헤더를 쉽게 추가하고 읽을 수 있는 메서드 제공.
      * 쿼리 파라미터를 쉽게 구성할 수 있는 빌더(Builder) 제공.
  * **오류 처리:** HTTP 상태 코드가 2xx가 아닐 경우, `ApiException` (가칭) 예외를 발생시켜 일관된 오류 처리 지원.

#### 2.2. 인증(Auth Type) 지원

라이브러리는 다양한 인증 방식을 쉽게 설정할 수 있는 메서드를 제공해야 합니다.
  * **Non Auth:**
      * 인증 없이 API 호출이 됩니다.  
  * **Basic Auth:**
      * `client.WithBasicAuth(string username, string password)`
      * `Authorization: Basic <base64(username:password)>` 헤더를 자동으로 추가합니다.
  * **Bearer Token:**
      * `client.WithBearerToken(string token)`
      * `Authorization: Bearer <token>` 헤더를 자동으로 추가합니다.
  * **API Key:**
      * `client.WithApiKey(string key, string value, ApiKeyLocation location = ApiKeyLocation.Header)`
      * `location` Enum (예: `Header`, `QueryParam`)을 통해 API 키의 위치를 지정할 수 있어야 합니다. (예: 헤더인 경우 `X-API-KEY: <value>`)
  * **OAuth 2.0:**
      * **Client Credentials Grant:**

          * `client.WithOAuthClientCredentials(string tokenEndpoint, string clientId, string clientSecret, string scope = null, OAuthOptions options = null)`
          * 지정된 엔드포인트로 토큰을 자동으로 요청하고, 만료 시 자동으로 갱신(Refresh)하는 로직을 내장합니다.
          * **OAuth 2.0 고급 기능 (프로덕션 필수):**
              * **토큰 캐싱 및 동시성 제어:**
                  * 다중 스레드 환경에서 토큰 갱신이 중복 실행되지 않도록 `SemaphoreSlim` 기반 락 메커니즘 적용
                  * Token Storm 방지를 위한 이중 체크 패턴(Double-Check Locking) 구현
              * **Clock Skew 보정:**
                  * 토큰 만료 시간(`expires_in`)에서 60~120초의 여유 시간을 차감하여 미리 갱신
                  * 네트워크 지연 및 서버 간 시간 불일치로 인한 인증 실패 방지
                  * 기본값: 120초 (OAuthOptions로 조정 가능)
              * **401 Unauthorized 자동 재시도:**
                  * 만료 추정 실패 케이스에서 `401` 응답 수신 시 1회에 한해 즉시 토큰 갱신 후 재시도
                  * 무한 루프 방지를 위한 재시도 횟수 제한 (요청당 1회)
              * **실패 시 지수 백오프(Exponential Backoff):**
                  * 토큰 갱신 실패 시 즉시 재시도하지 않고 점진적으로 대기 시간 증가
                  * 기본 정책: 초기 1초 → 2초 → 4초 (최대 3회 재시도)
              * **스레드 안전성:**
                  * `_token`, `_expiresAt` 등 공유 상태 변수에 대한 동기화 보장
                  * `async/await` 환경에서 데드락 방지를 위한 비동기 락 패턴 적용
              * **유연한 토큰 응답 파싱:**
                  * 대소문자 구분 없이 파싱 (`access_token` ↔ `accessToken`)
                  * 스네이크 케이스(snake_case) 및 카멜 케이스(camelCase) 모두 지원
              * **실패 텔레메트리:**
                  * 인증 실패 이벤트를 외부로 노출하는 이벤트/콜백 메커니즘 제공
                  * `OnAuthenticationFailure` 이벤트를 통한 모니터링 통합 지원

      * **Password Credentials Grant (Resource Owner Password Credentials):**

          * `client.WithOAuthPasswordCredentials(string tokenEndpoint, string username, string password, string clientId, string clientSecret = null)`
          * 사용자 ID/PW로 토큰을 요청하고 관리합니다. (권장되는 방식은 아니나, 레거시 지원을 위해 포함)

      * **Authorization Code Grant:**

          * 라이브러리가 브라우저 리디렉션과 같은 전체 흐름을 처리하는 것은 복잡합니다.
          * 대신, *외부에서* 획득한 `access_token`과 `refresh_token`을 설정하고, **토큰 갱신(Refresh Flow)을 자동화**하는 데 중점을 둡니다.
          * `client.WithOAuthAuthorizationCode(string tokenEndpoint, string refreshToken, string clientId, string clientSecret)`

#### 2.3. OAuthOptions 설정 클래스

OAuth 2.0 인증 동작을 세밀하게 제어하기 위한 옵션 클래스를 제공합니다.

```csharp
public class OAuthOptions
{
    /// <summary>
    /// Clock Skew 보정 시간 (초 단위, 기본값: 120초)
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 120;

    /// <summary>
    /// 토큰 갱신 실패 시 최대 재시도 횟수 (기본값: 3회)
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// 백오프 초기 지연 시간 (기본값: 1초)
    /// </summary>
    public TimeSpan InitialBackoffDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 401 응답 시 자동 재시도 활성화 여부 (기본값: true)
    /// </summary>
    public bool EnableAutoRetryOn401 { get; set; } = true;

    /// <summary>
    /// 시간 제공자 인터페이스 (테스트용 Mocking 가능)
    /// </summary>
    public ITimeProvider TimeProvider { get; set; } = new SystemTimeProvider();
}
```

-----

### 3\. 기술 사양 (라이브러리)

  * **개발 환경:** Visual Studio 2022 Professional
  * **언어:** C\#
  * **기반:** .NET Standard

#### 3.1. 프로젝트 분리 및 대상 프레임워크(TFM)

조건부 컴파일 지시문(`#if`)의 과도한 사용을 피하고 프로젝트의 복잡성을 낮추기 위해, 라이브러리를 두 개의 개별 프로젝트로 분리합니다.

*   **`RestFlow.Client10`**: 구형 프레임워크 지원 (API 제약이 있는 버전)
*   **`RestFlow.Client20`**: 최신 및 주요 프레임워크 지원

이 접근 방식은 각 환경에 최적화된 코드를 유지하고, 복잡한 조건부 로직 없이 더 깔끔한 개발을 가능하게 합니다.

##### 3.1.1. RestFlow.Client10 프로젝트

> API가 제한적인 구형 프레임워크를 지원합니다.

*   **대상 프레임워크 (TargetFrameworks):** `netstandard1.0;net452`
*   **`.csproj` 파일 설정 예시:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard1.0;net452</TargetFrameworks>
    <LangVersion>7.3</LangVersion>
    <PackageId>RestFlow.Client10</PackageId>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Net.Http" Version="4.3.4" />
    <PackageReference Include="Newtonsoft.Json" Version="12.0.3" />
  </ItemGroup>
</Project>
```

##### 3.1.2. RestFlow.Client20 프로젝트

> `netstandard2.0`을 기반으로 하여 광범위한 최신 .NET 환경을 지원합니다.

*   **대상 프레임워크 (TargetFrameworks):** `netstandard2.0;`
*   **`.csproj` 파일 설정 예시:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;netstandard2.1;net461;net472;net48</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <PackageId>RestFlow.Client20</PackageId>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>

  <ItemGroup>
    <!-- netstandard2.0 이상에서는 System.Net.Http가 기본 포함됩니다. -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
  </ItemGroup>

</Project>
```

**구현 참고:**

*   두 프로젝트는 별도의 NuGet 패키지(`RestFlow.Client10`, `RestFlow.Client20`)로 배포됩니다.
*   공통 로직이 많을 경우, **공유 프로젝트(Shared Project)**를 활용하여 코드 중복을 최소화할 수 있습니다.

-----

### 4\. API 테스트 프로그램 (Test Library)

`RestFlow` 라이브러리의 각 프로젝트가 올바르게 작동하는지 검증하기 위해 별도의 테스트 프로젝트를 각각 생성합니다.

#### 4.1. RestFlow.Client10.Tests

`RestFlow.Client10` 라이브러리를 테스트합니다.

*   **프로젝트명:** `RestFlow.Client10.Tests`
*   **프로젝트 타입:** xUnit 테스트 프로젝트
*   **대상 프레임워크:** `net452`
*   **테스트 대상:** `RestFlow.Client10`

#### 4.2. RestFlow.Client20.Tests

`RestFlow.Client20` 라이브러리를 테스트합니다.

*   **프로젝트명:** `RestFlow.Client20.Tests`
*   **프로젝트 타입:** xUnit 테스트 프로젝트
*   **대상 프레임워크:** `net8.0` (최신 환경에서 테스트)
*   **테스트 대상:** `RestFlow.Client20`

#### 4.3. 공통 테스트 요구사항

*   **Mocking:** `WireMock.Net` 또는 `Moq` 라이브러리를 사용하여 실제 HTTP 서버 없이도 API 응답(성공, 실패, 인증 오류 등)을 시뮬레이션합니다.
*   **테스트 케이스 (Auth Type 별):**
    *   **NonAuth\_Tests:**
        *   `NonAuth_설정_시_인증없이_호출이_되는지_검증`
    *   **BasicAuth\_Tests:**
        *   `BasicAuth_설정_시_올바른_Authorization_헤더가_생성되는지_검증`
    *   **BearerToken\_Tests:**
        *   `BearerToken_설정_시_올바른_Authorization_헤더가_생성되는지_검증`
    *   **ApiKey\_Tests:**
        *   `ApiKey_Location이_Header일때_지정된_헤더에_키가_추가되는지_검증`
        *   `ApiKey_Location이_QueryParam일때_URI에_쿼리파라미터가_추가되는지_검증`
    *   **OAuth2\_ClientCredentials\_Tests:**
        *   `Mock_토큰_엔드포인트에서_정상적으로_토큰을_요청하고_가져오는지_검증`
        *   `획득한_토큰이_API_요청시_Bearer_헤더에_포함되는지_검증`
        *   `토큰_만료_시_자동으로_갱신_로직이_호출되는지_검증`
        *   `ClockSkew_적용_시_만료_이전에_미리_갱신되는지_검증`
        *   `다중_스레드_환경에서_토큰_갱신이_중복_실행되지_않는지_검증` (동시성 제어)
        *   `401_응답_수신_시_1회_자동_재시도가_동작하는지_검증`
        *   `401_재시도가_1회로_제한되어_무한_루프가_발생하지_않는지_검증`
        *   `토큰_갱신_실패_시_지수_백오프가_적용되는지_검증`
        *   `대소문자_및_스네이크_케이스_토큰_응답을_올바르게_파싱하는지_검증`
        *   `인증_실패_이벤트가_정상적으로_발생하는지_검증` (텔레메트리)
    *   **OAuth2\_PasswordCredentials\_Tests:**
        *   `Username과_Password로_정상적으로_토큰을_요청하는지_검증`
    *   **OAuth2\_AuthorizationCode\_Tests:**
        *   `RefreshToken을_이용하여_새로운_AccessToken을_정상적으로_갱신하는지_검증`

#### 4.4. (선택 사항) 테스트용 GUI 프로그램

라이브러리를 실제 환경에서 쉽게 테스트하고 시연(Demo)할 수 있는 간단한 WPF 애플리케이션을 제공합니다.

*   **프로젝트명:** `RestFlow.Client10.TestApp.Wpf`, `RestFlow.Client20.TestApp.Wpf`
*   **대상 프레임워크:** `net452` (`Client10`용), `net8.0-windows` (`Client20`용)
*   **공통 기능:**
    *   Auth Type 선택 드롭다운 (Basic, Bearer, OAuth 등).
    *   관련 자격증명(ID/PW, Client ID/Secret 등) 입력 필드.
    *   API Endpoint, Method(GET/POST) 입력 필드.
    *   '요청' 버튼.
    *   Request/Response 헤더 및 본문(Body)을 보여주는 텍스트 영역.

-----

### 5\. 주요 산출물

1.  **`RestFlow.sln`:** Visual Studio 2022 솔루션 파일.
2.  **`RestFlow.Client10`:** C# 클래스 라이브러리 프로젝트. (미구현)
3.  **`RestFlow.Client20`:** C# 클래스 라이브러리 프로젝트.
4.  **`RestFlow.Client10.Tests`:** xUnit 테스트 프로젝트. (미구현)
5.  **`RestFlow.Client20.Tests`:** xUnit 테스트 프로젝트.
6.  **(선택) `RestFlow.Client10.TestApp.Wpf`:** WPF 기반 테스트 애플리케이션. (미구현)
7.  **(선택) `RestFlow.Client20.TestApp.Wpf`:** WPF 기반 테스트 애플리케이션. (미구현)
8.  **`README.md`:** 라이브러리 사용법, 설치 방법, 각 인증 방식별 예제 코드가 포함된 문서.