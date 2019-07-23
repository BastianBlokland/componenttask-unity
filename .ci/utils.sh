#!/bin/bash
set -e

# --------------------------------------------------------------------------------------------------
# Miscellaneous utilities that can be used by other bash scripts.
# --------------------------------------------------------------------------------------------------

info()
{
    if [ -z "$NO_COLORS" ]
    then
        GREEN='\033[0;32m'
        NORMAL='\033[0m'
        echo -e "${GREEN}INFO: ${1}${NORMAL}"
    else
        echo "INFO: $1"
    fi
}

warn()
{
    if [ -z "$NO_COLORS" ]
    then
        YELLOW='\033[0;33m'
        NORMAL='\033[0m'
        echo -e "${YELLOW}WARN: ${1}${NORMAL}"
    else
        echo "WARN: $1"
    fi
}

fail()
{
    if [ -z "$NO_COLORS" ]
    then
        RED='\033[0;31m'
        NORMAL='\033[0m'
        echo -e "${RED}ERROR: ${1}${NORMAL}" >&2
    else
        echo -e "ERROR: $1" >&2
    fi
    exit 1
}

hasCommand()
{
    if [ -x "$(command -v $1)" ]
    then
        return 0
    fi
    return 1
}

doesntHaveCommand()
{
    if hasCommand $1
    then
        return 1
    fi
    return 0
}

verifyCommand()
{
    if doesntHaveCommand $1
    then
        fail "Dependency '$1' is missing, we cannot continue without it."
    fi
}

ensureDir()
{
    if [ ! -d "$1" ]
    then
        mkdir "$1"
    fi
}

withRetry()
{
    local n=1
    local max=3
    local delay=5
    while true
    do "$@" && break ||
    {
        if [[ $n -lt $max ]]
        then
            ((n++))
            warn "Command '$*' failed. Attempt '$n/$max':"
            sleep $delay;
        else
            fail "Command '$*' has failed after '$n' attempts."
        fi
    }
    done
}

logDuration()
{
    # Save start-time, execute command and log elapsed time.
    local startTime=$SECONDS
    "$@" ||
    {
        warn "Command '$*' failed in '$((SECONDS - startTime))' seconds."
        return 1
    }

    info "Command '$*' succeeded in '$((SECONDS - startTime))' seconds."
    return 0
}
