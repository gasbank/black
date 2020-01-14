#!/bin/bash

API_BASE="$1"
INPUT_FILE="$2"

if [ -z "${API_BASE}" ] || [ -z "${INPUT_FILE}" ]; then
  echo "$0 api-base input-file"
  exit 0
fi

if [ "${INPUT_FILE: -4}" != ".png" ]; then
  echo "Please use png file as an input."
  exit 1
fi

FILE_KEY="$(basename "${INPUT_FILE}" ".png")"

echo "Delete old file."
curl -SsL -XDELETE "${API_BASE}/${FILE_KEY}" > /dev/null

echo "Retrieve an upload URL."
UPLOAD_URL="$(curl -SsL -XPUT "${API_BASE}/${FILE_KEY}")"

if [ -z "${UPLOAD_URL}" ]; then
  echo "Cannot create upload URL."
  exit 1
fi

echo "Upload [${INPUT_FILE}] file."
curl -T "${INPUT_FILE}" "${UPLOAD_URL}"
if [ $? -ne 0 ]; then
  echo "Cannot upload ${INPUT_FILE} file."
  exit 1
fi

echo "Wait 20 seconds until processing..."
sleep 20

for I in $(seq 10); do
  echo "Check completion..."
  RET="$(curl -SsL -XGET "${API_BASE}/${FILE_KEY}")"
  if [ ! -z "${RET}" ]; then
    echo "All done."
    which jq > /dev/null
    if [ $? -eq 0 ]; then
      echo "${RET}" | jq .
    else
      echo "${RET}"
    fi
    exit 0
  fi
  sleep 2
done

echo "Maybe error occurred while transforming."
echo "Please check the server log."
exit 1

