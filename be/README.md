# Backend for Black

## transform

이미지를 게임에 사용할 수 있는 형태로 가공해주는 함수를 가지고 있는 DLL Project

## transform-cli

기존 dev-tools와 동일한 실행 인자를 갖는 CLI Project로 transform project를 참조

## transform-api

transform DLL을 AWS Lambda에서 사용하기 위한 Serverless Handler Project. 지정된 S3의 지정된 위치에 파일을 올리면 지정된 위치에 결과를 업로드한다.

## Development

transform, transform-cli는 다음과 같이 빌드, 실행할 수 있다.

```bash
dotnet restore
dotnet build
dotnet publish
```

transform-api는 `.envrc` 파일을 적당히 설정한 후 AWS profile을 설정한 다음 다음과 같이 배포할 수 있다.

```bash
./build.sh
sls deploy
```

## transform-proxy (WIP)

transform-api에 의해 이미지를 변환하기 위해 S3에 업로드할 대상의 주소를 획득하는 API를 제공하고, 생성된 결과물을 조회할 수 있는 API를 제공하는 Serverless project로 현재 개발 중이다.

