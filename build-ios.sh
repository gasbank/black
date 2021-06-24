#!/bin/bash

UNITY_VERSION=`cat ProjectSettings/ProjectVersion.txt | head -n1 | awk '{print $2;}'`

if [ -z "${BUILD_NUMBER}" ]; then
  BUILD_NUMBER="<NO ENV>"
fi

/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity \
    -quit \
    -batchmode \
    -executeMethod BlackBuild.PerformIosBuild \
    -logfile build.log \
    -projectPath `pwd` \
    -buildTarget iOS \
    -buildNumber ${BUILD_NUMBER} \
    -noGraphics \
    -username ${UNITY_USERNAME} \
    -password ${UNITY_PASSWORD}
