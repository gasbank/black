$BOT_TOKEN=$env:BOT_TOKEN

$BUILD_TYPE='AndroidWindows'

$DOWNLOAD_URL_RESULT_ARMEABI_V7A=$(Invoke-WebRequest -UseBasicParsing -s -XGET https://api.yyt.life/d/black/android-armeabi-v7a?count=1)
$DOWNLOAD_URL_RESULT_ARM64_V8A=$(Invoke-WebRequest -UseBasicParsing -s -XGET https://api.yyt.life/d/black/android-arm64-v8a?count=1)
$DOWNLOAD_URL_RESULT_X86=$(Invoke-WebRequest -UseBasicParsing -s -XGET https://api.yyt.life/d/black/android-x86?count=1)

# *****************
$DOWNLOAD_URL_ARMEABI_V7A=$(Write-Output $DOWNLOAD_URL_RESULT_ARMEABI_V7A.content | ConvertFrom-Json).versions[0]
$DOWNLOAD_URL_ARM64_V8A=$(Write-Output $DOWNLOAD_URL_RESULT_ARM64_V8A.content | ConvertFrom-Json).versions[0]
$DOWNLOAD_URL_X86=$(Write-Output $DOWNLOAD_URL_RESULT_X86.content | ConvertFrom-Json).versions[0]
# *****************
$BUILD_NUMBER=$env:BUILD_NUMBER #'%build.number%'

$SEND_MESSAGE_URL = "https://api.telegram.org/bot$BOT_TOKEN/sendMessage"

# $HEADERS = @{
#     'Content-Type' = 'text/plain'
# }

$MESSAGE = "Build Number: $BUILD_NUMBER\nBuild Type: $BUILD_TYPE\nDownload: <a href='$DOWNLOAD_URL_ARMEABI_V7A'>ARMEABI_V7A</a> / <a href='$DOWNLOAD_URL_ARM64_V8A'>ARM64_V8A</a> / <a href='$DOWNLOAD_URL_X86'>X86</a>"

Write-Output $MESSAGE

$SEND_MESSAGE_PARAMS = @{
    chat_id = $env:CHAT_ID
    text = "Build completed.\n$MESSAGE"
    parse_mode = 'HTML'
}

$SEND_MESSAGE_PARAMS_UTF8 = [System.Text.Encoding]::UTF8.GetBytes(($SEND_MESSAGE_PARAMS|ConvertTo-Json).Replace('\\n','\n'));

# plusalpha.top 경유해서 텔래그램 메시지 보내는 방법 - 이제 쓰지 말고 직접 보내자.
# Invoke-WebRequest -Method POST -Headers $HEADERS -Body $MESSAGE -Uri "https://plusalpha.top/sushi-telegram/$BOT_TOKEN/teamcity-build-event"
# 이게 직접 보내는 방법
Invoke-WebRequest -UseBasicParsing -Method POST -Uri $SEND_MESSAGE_URL -ContentType "application/json" -Body $SEND_MESSAGE_PARAMS_UTF8


$DEV_DEVICE_FCM_TOKEN="cYNTYuvuQmE:APA91bEycsmgL4FBW5ggbiHe28XbpkVYD13MAr3JqyOtH3k-zWDQTN4BJPfzj0GgtdBldTnvZ4NPcsr57qUclU8fRv7FzuSo8YvGyMjOnIItcSKxEMhtMTvRp_DSFIVDAc0Y3ftczCtM"
$PACKAGE_NAME="top.plusalpha.black"
node C:\sushi-dev-tools\sulchi-server\index.js $BUILD_NUMBER $DOWNLOAD_URL_ARM64_V8A $DEV_DEVICE_FCM_TOKEN $PACKAGE_NAME
