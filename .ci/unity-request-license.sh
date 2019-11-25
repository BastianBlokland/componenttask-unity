#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Create a Unity 'Manual Activation File'.
#
# Will output 'output/licenseActivateFile.alf' file that can be used to request a license file at:
# https://license.unity3d.com/manual
# --------------------------------------------------------------------------------------------------

generateManualActivationFile()
{
    local projPath="$1"
    local outputPath="$2"
    local logPath="$3"

    # delete previous output.
    rm -f "$outputPath"

    info "Running 'createManualActivationFile' in project '$projPath' (logging to: '$logPath')."
    (cd "$UNITY_PROJ_DIR" &&
    {
        unity \
            -batchmode \
            -quit \
            -createManualActivationFile \
            -logfile "$LOG_PATH" \
            -nographics \
            -force-free || true
        info "Unity exited."
    })

    # Find the created '.alf' file and copy it to the output path.
    find "$projPath" -name "*.alf" -exec cp {} "$outputPath" \;
    if [ -f "$outputPath" ]
    then
        info "Sucesfully created license-activation file at: '$outputPath'."
        return 0
    else
        warn "Failed to create license-activation file."
        return 1
    fi
}

# Verify dependencies are present.
verifyCommand find

# Setup variables.
UNITY_PROJ_DIR=".example"
if [ ! -d "$UNITY_PROJ_DIR" ]
then
    fail "No directory found at: '$UNITY_PROJ_DIR'."
fi
OUTPUT_PATH="$(pwd)/.output/licenseActivateFile.alf"
LOG_PATH="$(pwd)/.output/request-license.log"

# Generate license activation file (.alf).
withRetry logDuration generateManualActivationFile "$UNITY_PROJ_DIR" "$OUTPUT_PATH" "$LOG_PATH"

info "Finished creating a Unity activation file."
exit 0
