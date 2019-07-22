#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Install required dependencies.
# --------------------------------------------------------------------------------------------------

installRuby()
{
    info "Atempting to install 'ruby'."
    if hasCommand apt-get
    then
        info "'apt-get' package manager found at: '$(which apt-get)'."
        withRetry logDuration sudo apt-get update
        withRetry logDuration sudo apt-get install ruby-full
        if hasCommand gem
        then
            info "Succesfully installed 'ruby'."
            return 0
        else
            fail "Failed to install 'ruby' with 'apt-get'."
        fi
    fi
    fail "Failed to install ruby as no supported package manager is installed."
}

getUnityVersion()
{
    local projectDir="$1"
    local projectVersionFile="$projectDir/ProjectSettings/ProjectVersion.txt"
    if [ ! -f "$projectVersionFile" ]
    then
        echo "Unknown"
        return 1
    fi
    echo "$(awk '$1 == "m_EditorVersion:" {print $2}' $projectVersionFile)"
    return 0
}

installUnity()
{
    local version="$1"
    info "Installing Unity '$version' with 'u3d'."
    sudo u3d install $version
}

if doesntHaveCommand awk
then
    fail "'awk' is required, we cannot proceed without it."
else
    info "'awk' found at: '$(which awk)'."
fi

if doesntHaveCommand gem
then
    installRuby
else
    info "'gem' found at: '$(which gem)'."
fi

if doesntHaveCommand u3d
then
    info "'u3d' not installed, installing with 'gem'."
    withRetry logDuration sudo gem install u3d
else
    info "'u3d' found at: '$(which u3d)', attempting to update through 'gem':"
    withRetry logDuration sudo gem update u3d
    info "Updated 'u3d' through 'gem'."
fi

UNITY_PROJ_DIR=".example"
if [ ! -d "$UNITY_PROJ_DIR" ]
then
    fail "No directory found at: '$UNITY_PROJ_DIR'."
fi

unityVersion="$(getUnityVersion "$UNITY_PROJ_DIR")"
withRetry logDuration installUnity "$unityVersion"

info "Sucesfully installed all dependencies."
exit 0
