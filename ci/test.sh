#!/usr/bin/env bash

# Adapted from https://gitlab.com/gableroux/unity3d-gitlab-ci-example/-/blob/master/ci/test.sh

set -x

echo "Testing for $TEST_PLATFORM"


${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $(pwd) \
  -runTests \
  -testPlatform $TEST_PLATFORM \
  -testResults $TEST_PLATFORM-results.xml \
  -logFile /dev/stdout \
  -batchmode \
  -nographics


UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

echo "-----------------------------------------"
echo "Output ${TEST_PLATFORM}-results.xml"
cat $(pwd)/${TEST_PLATFORM}-results.xml
exit $UNITY_EXIT_CODE
