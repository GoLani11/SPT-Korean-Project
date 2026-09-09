# SPT Korean Localization / SPT 한글화 프로젝트

**2.1.0 배포 준비 완료 — 공개 업로드 대기**

[Releases](../../releases) · [Issues](../../issues) · [설치 안내](docs/releases/2.1.0-install.md)

Golani가 제작하고 Gomeng이 배포를 돕는 SPT 한글화 프로젝트입니다. **`SPT-KR-2.1.0.zip` 하나**에 기본 한글판, 전체 한영 병기판과 한국어 UI 보정을 제공합니다.

## 설치와 업데이트

1. 게임과 서버를 종료합니다.
2. 기존 서버 한글화 모드, 테스트용 서버 안내 모듈과 중복 클라이언트 DLL을 모드 검색 경로 밖에 백업한 후 제거합니다. 정확한 경로는 [설치 안내](docs/releases/2.1.0-install.md)를 따르세요.
3. ZIP 안의 **BepInEx 폴더를 `EscapeFromTarkov.exe`가 있는 게임 폴더로** 끌어다 놓습니다.
4. 평소처럼 서버와 게임을 실행하고 `설정 > 게임 > 인터페이스 언어`에서 `한국어 (Korean)` 또는 `한국어 (한영 병기)`를 선택합니다.

두 방식은 게임 재시작이나 재설치 없이 전환할 수 있습니다. ZIP은 `BepInEx/plugins` 아래의 DLL·번역·안내·라이선스만 포함합니다. 설치 프로그램과 서버 모듈은 없습니다.

서버 콘솔 안내 대신 게임의 `BepInEx/LogOutput.log`에 모드 버전과 번역 초기화 결과를 기록합니다. 테스트판의 2.2.0 표시는 정식 배포 번호인 2.1.0으로 정리했으며, 최근 테스트한 기능을 포함합니다.

## 지원 범위

번역 프로필은 **3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, 4.1.0, 4.1.2, 4.1.3, 4.1.5**입니다. 플러그인이 설치 버전을 확인해 번역을 선택합니다.

미등록 안정판 4.1.x는 4.1.2 이상~4.2.0 미만에서 게임 빌드·영어 원문·필요한 게임 함수가 호환될 때만 기존 프로필을 사용합니다. 4.1.1, 시험판, 4.2 이상은 제외합니다. **같은 설치 폴더에 해당 버전의 로컬 SPT 서버 데이터가 필요합니다.**

3.8.3·3.9.8·3.10.5·3.11.4·4.1.5의 설치 파일과 Unity Mono 실행 환경을 자동 검증했습니다. 나머지 프로필은 번역 데이터와 모의 동작을 검증했으며, 실제 게임 화면 검증을 뜻하지 않습니다. 서버나 다른 모드가 직접 만든 일부 문장은 번역되지 않을 수 있습니다.

상세 변경 사항은 [2.1.0 릴리즈 본문](docs/releases/2.1.0-release-notes.md), 구현과 검증 범위는 [클라이언트 한글화 문서](docs/client-locale-prototype.md)를 참고하세요.

## 빌드

.NET 10 SDK, Python, Windows .NET Framework 4.8, SPT 3.8.3 참조 클라이언트와 같은 상위 폴더의 `spt-korean-translate` 저장소가 필요합니다. WSL에서는 Windows 연동을 사용합니다.

```powershell
python .\tools\package_release.py --spt-383-root D:\SPT_3.8.3 --spt-415-root D:\SPT
```

또는 `make-release-packages.bat`을 실행합니다. 결과는 `artifacts/release-2.1.0`에 저장되며 Git에 포함하지 않습니다. 배포 경로는 공통 ZIP 한 개를 생성하고, Windows 및 발견된 게임의 Unity Mono 환경에서 검증한 뒤 압축을 다시 풀어 검사합니다. 이전 서버 배포 프로젝트와 `package_release_versions.py`는 과거 구현이며 현재 릴리즈 경로가 아닙니다.

## English

Version 2.1.0 is prepared for publication. Install the single `SPT-KR-2.1.0.zip` by copying its BepInEx folder into the game root. Back up and remove the previous Korean server mod and duplicate plugin DLLs first. Both Korean and Korean–English modes are available in the native language setting. The matching local SPT server database is required. See the linked installation guide for supported profiles and verification limits.

## 라이선스 및 제보

소스 코드는 [MIT License](LICENSE.md)를 따릅니다. 배포 ZIP에 모드·클라이언트·번역 라이선스를 함께 제공합니다. 클라이언트 코드를 재사용할 때 프로젝트 이름과 저장소 주소를 남겨 주세요.

오역이나 오류는 SPT 버전, 언어 모드와 발생 위치를 [Issues](../../issues)에 남겨 주세요.
