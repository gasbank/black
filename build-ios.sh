#!/bin/bash

if [ -z "${BUILD_NUMBER}" ]; then
  BUILD_NUMBER="<NO ENV>"
fi

/Applications/Unity/Hub/Editor/2019.2.9f1/Unity.app/Contents/MacOS/Unity \
    -quit \
    -batchmode \
    -executeMethod BlackBuild.PerformIosBuild \
    -logfile build.log \
    -projectPath `pwd` \
    -buildTarget iOS \
    -buildNumber ${BUILD_NUMBER} \
    -noGraphics
