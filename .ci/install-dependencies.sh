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
        withRetry logDuration sudo apt-add-repository ppa:brightbox/ruby-ng
        withRetry logDuration sudo apt-get update
        withRetry logDuration sudo apt-get install ruby2.5
        if hasCommand gem
        then
            info "Succesfully installed 'ruby'."
            return 0
        else
            fail "Failed to install 'ruby' with 'apt-get'."
        fi
    fi
    fail "Failed to install 'ruby' as no supported package manager is installed."
}

getUnityVersion()
{
    local projectDir="$1"
    local projectVersionFile="$projectDir/ProjectSettings/ProjectVersion.txt"
    if [ ! -f "$projectVersionFile" ]
    then
        echo "unknown"
        return 1
    fi
    echo "$(awk '$1 == "m_EditorVersion:" {print $2}' $projectVersionFile)"
    return 0
}

installUnity()
{
    local version="$1"
    info "Installing Unity '$version' with 'u3d'."

    # HACK: Ignoring exit code of the install. Reason is that u3d at this moment is incompatible
    # with unity's new module definitions (u3d issue: https://github.com/DragonBox/u3d/issues/391).
    # However even though it fails it has actually installed Unity correctly so as a work-around we
    # ignore the error here and in all Unity invocations we supply the full path.
    # TODO: Remove this as soon as the issue has been resolved on the u3d side.
    (sudo u3d install $version -p Unity --trace) || true
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

# List 'u3d' version.
info "'u3d' version:"
u3d --version

UNITY_PROJ_DIR=".example"
if [ ! -d "$UNITY_PROJ_DIR" ]
then
    fail "No directory found at: '$UNITY_PROJ_DIR'."
fi

# List all available linux unity versions (helps in debugging unavailable unity versions).
withRetry u3d available -o linux --no-central --force

unityVersion="$(getUnityVersion "$UNITY_PROJ_DIR")"
withRetry logDuration installUnity "$unityVersion"

info "Sucesfully installed all dependencies."
exit 0
