#!/bin/bash

TARGET_CSHARP_FILE=Assets/Scripts/BlackMsgPack.cs

# convert
$MPC_PATH -i Assembly-CSharp.csproj -o $TARGET_CSHARP_FILE

# fix line ending style
perl -pi -e 's/\r\n|\n|\r/\n/g' $TARGET_CSHARP_FILE

