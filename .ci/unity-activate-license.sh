#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Activate a Unity license.
# --------------------------------------------------------------------------------------------------

hasLicense()
{
    if [ -z "$(u3d licenses)" ]
    then
        return 1
    fi
    return 0
}

activateUnity()
{
    local projDir="$1"
    local licenseFilePath="$2"
    local logPath="$3"

    info "Attempting to activate using '$licenseFilePath' in project '$projDir' (logging to: '$logPath')."
    (cd "$projDir" &&
    {
        u3d -- \
            -batchmode \
            -logfile "$logPath" \
            -quit \
            -nographics \
            -force-free \
            -manualLicenseFile "$licenseFilePath" || true
        info "Unity exited."
    })

    if hasLicense
    then
        return 0
    else
        warn "Failed to activate unity."
        return 1
    fi
}

# Verify dependencies.
verifyCommand u3d

# Early out if already activated.
if hasLicense
then
    info "Unity already activated, skipping activation."
    exit 0
fi

# Setup variables.
UNITY_PROJ_DIR=".example"
if [ ! -d "$UNITY_PROJ_DIR" ]
then
    fail "No directory found at: '$UNITY_PROJ_DIR'."
fi
UNITY_LICENSE_FILE_PATH="$1"
if  [ -z "$UNITY_LICENSE_FILE_PATH" ]
then
    fail "No license file path provided, please provide as arg1."
fi
LOG_PATH="$(pwd)/output/activation.log"

# Activate unity with given license file.
withRetry logDuration activateUnity "$UNITY_PROJ_DIR" "$UNITY_LICENSE_FILE_PATH" "$LOG_PATH"

info "Sucesfully activated unity."
exit 0
