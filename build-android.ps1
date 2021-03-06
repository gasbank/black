# 1. Copy key file
Copy-Item -Force -Recurse -Verbose C:\black\key .

# 2. Remove Previous APK
Remove-Item -Force -ErrorAction:Ignore -Recurse -Verbose black.apk

$BUILD_LOG_FILE_NAME = 'build.log'

# 3. Build APK
if (-not (Test-Path env:DUMMY_BUILD)) {
    if (-not (Test-Path env:BUILD_NUMBER)) {
        $env:BUILD_NUMBER = '<NO ENV>'
    }

    & "C:\Program Files\Unity\Hub\Editor\2019.2.17f1\Editor\Unity.exe" `
        -quit `
        -batchmode `
        -executeMethod BlackBuild.PerformAndroidBuild `
        -logfile $BUILD_LOG_FILE_NAME `
        -projectPath $pwd `
        -buildTarget Android `
        -keystorePass $env:KEYSTORE_PASS `
        -buildNumber $env:BUILD_NUMBER `
        -noGraphics | Out-Null
} else {
    New-Item -Force -Verbose $BUILD_LOG_FILE_NAME
    Set-Content $BUILD_LOG_FILE_NAME -Verbose 'test build.log content'

    New-Item -Force -Verbose black.apk
    Set-Content black.apk -Verbose 'test apk file content'
}

# 4. Check build.log
$OK_LINES = (Get-Content build.log | Select-String "DisplayProgressNotification: Build Successful")
$OK_COUNT = $OK_LINES.length
if ($OK_COUNT -ne 0) {
    # Build successful!
} else {
    $ERROR_LINES = (Get-Content build.log | Select-String "\): error CS")
    $ERROR_COUNT = $ERROR_LINES.length
    if ($ERROR_COUNT -ne 0) {
        Write-Output "$ERROR_COUNT lines detected!"
        Write-Output $ERROR_LINES
        exit $ERROR_COUNT
    }

    $ERROR_LINES = (Get-Content build.log | Select-String " Error: ")
    $ERROR_COUNT = $ERROR_LINES.length
    if ($ERROR_COUNT -ne 0) {
        Write-Output "$ERROR_COUNT lines detected!"
        Write-Output $ERROR_LINES
        exit $ERROR_COUNT
    }

    $ERROR_LINES = (Get-Content build.log | Select-String ": Build Failed")
    $ERROR_COUNT = $ERROR_LINES.length
    if ($ERROR_COUNT -ne 0) {
        Write-Output "$ERROR_COUNT lines detected!"
        Write-Output $ERROR_LINES
        exit $ERROR_COUNT
    }
}

# 5. Upload APK (disabled)
####

# 6. Deploy to d.yyt.life
.\upload-to-yyt.ps1 black android-armeabi-v7a top.plusalpha.black "black.apk\Black.armeabi-v7a.apk"
.\upload-to-yyt.ps1 black android-arm64-v8a top.plusalpha.black "black.apk\Black.arm64-v8a.apk"
#.\upload-to-yyt.ps1 black android-x86 top.plusalpha.black "black.apk\Black.x86.apk"

# 7.  Notify Developers on Telegram
.\notify-telegram-android-windows.ps1
