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
    local userName="$1"
    local userPassword="$2"
    local exampleDir=".example"
    local logPath="$(pwd)/output/activation.log"

    info "Attempting to activate Unity."
    (cd "$exampleDir" &&
    {
        u3d -- \
            -batchmode \
            -logfile "$logPath" \
            -quit \
            -nographics \
            -force-free \
            -username "$userName" \
            -password "$userPassword"
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

if doesntHaveCommand u3d
then
    fail "'u3d' required (more info: https://github.com/DragonBox/u3d)."
fi

if hasLicense
then
    info "Unity already activated, skipping activation."
    exit 0
fi

UNITY_USER="$1"
if  [ -z "$UNITY_USER" ]
then
    fail "No unity username provided, please provide as arg1."
fi

UNITY_PASS="$2"
if  [ -z "$UNITY_PASS" ]
then
    fail "No unity password provided, please provide as arg2."
fi

withRetry logDuration activateUnity "$UNITY_USER" "$UNITY_PASS"

info "Sucesfully activated unity."
exit 0
