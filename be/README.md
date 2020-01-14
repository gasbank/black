# Backend for Black

`transform-api`와 `transform-s3-api`를 배포한 이후 다음과 같이 실행할 수 있다.

```bash
./black-transform.sh S3_API_URL FILE_NAME
```

`transform-api`는 S3 이벤트에 의해 실행되므로, `S3_API_URL`은 `transform-s3-api`의 주소이다. 만약 이 주소가 `https://api.your.domain/dev` 이고 `input.png`에 대해 변환을 수행한다면 다음과 같이 실행하면 된다.

```bash
./black-transform.sh https://api.your.domain/dev input.png
```

그럼 다음과 같이 약 30초 후에 결과를 받을 수 있는 URL이 출력된다.

```text
Delete old file.
Retrieve an upload URL.
Upload [/home/lacti/tmp/input.png] file.
Wait 20 seconds until processing...
Check completion...
Check completion...
Check completion...
Check completion...
Check completion...
All done.
[
  "http://YOUR-S3-BUCKET-NAME.s3-website.AWS-REGION.amazonaws.com/result/input.bytes",
  "http://YOUR-S3-BUCKET-NAME.s3-website.AWS-REGION.amazonaws.com/result/input-OTB-FSNB.png",
  "http://YOUR-S3-BUCKET-NAME.s3-website.AWS-REGION.amazonaws.com/result/input-OTB-FSNB-BB-SDF.png"
]
```

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

## transform-s3-api

transform-api에 의해 이미지를 변환하기 위해 S3에 업로드할 대상의 주소를 획득하는 API를 제공하고, 생성된 결과물을 조회할 수 있는 API를 제공한다.

