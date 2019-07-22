#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Install required dependencies.
# --------------------------------------------------------------------------------------------------

if doesntHaveCommand sed
then
    fail "'sed' is required, we cannot proceed without it."
else
    info "'sed' found at: '$(which sed)'"
fi

if doesntHaveCommand gem
then
    fail "RubyGems is required, we cannot proceed without it."
else
    info "'gem' found at: '$(which gem)'"
fi

if doesntHaveCommand u3d
then
    info "'u3d' not installed, installing with 'gem'"
    withRetry gem install u3d
else
    info "'u3d' found at: '$(which u3d)'"
fi

getUnityVersion ()
{
    local projectDir="$1"
    local projectVersionFile="$projectDir/ProjectSettings/ProjectVersion.txt"
    if [ ! -f "$projectVersionFile" ]
    then
        echo "Unknown"
        return 1
    fi
    echo "$(sed -e 's/[^ ]* //' $projectVersionFile)"
    return 0
}

unityVersion="$(getUnityVersion .example)"

info "Installing Unity '$unityVersion' with 'u3d'"
u3d install $unityVersion

info "Sucesfully installed all dependencies"
exit 0
