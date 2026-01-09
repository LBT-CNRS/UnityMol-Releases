#!/usr/bin/env bash

# Exit immediately if a command returns a non-zero error code
set -e
set -x
mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/
# Don't print trace of simple commands
set +x

unity_license_destination=/root/.local/share/unity3d/Unity/Unity_lic.ulf

# dependency for msms
apt update && apt install -y lib32z1

if [ -n "$UNITY_LICENSE" ]
then
    echo "Writing '\$UNITY_LICENSE' to license file ${unity_license_destination}"
    echo "${UNITY_LICENSE}" | tr -d '\r' > ${unity_license_destination}
else
    echo "'\$UNITY_LICENSE' env var not found"
fi

# Manually add Photon Voice 2 Asset in the project
# It should be done through Unity Script API (AssetDatabase.ImportPackage) or CLi ('-importPackage /path') but
# none of them work. (It seems those calls are asyncrhonous and scripts get compiled before the package is imported
# therefore compilation errors arises.)

wget $PHOTONVOICE_URL -O /root/photon.tar.gz
tar -xzvf /root/photon.tar.gz -C  $(pwd)/Assets/
