#!/bin/bash
set -e

# --------------------------------------------------------------------------------------------------
# Miscellaneous reusable utilities that can be used by other bash scripts.
# --------------------------------------------------------------------------------------------------

info ()
{
    echo -e "\033[0;32mINFO: $1\033[0m"
}

warn ()
{
    echo -e "\033[0;33mWARN: $1\033[0m"
}

fail ()
{
    echo -e "\033[0;31mERROR: $1\033[0m" >&2
    exit 1
}

hasCommand ()
{
    if [ -x "$(command -v $1)" ]
    then
        return 1
    fi
    return 0
}

doesntHaveCommand ()
{
    if hasCommand $1
    then
        return 0
    fi
    return 1
}

verifyCommand ()
{
    if doesntHaveCommand $1
    then
        fail "Dependency '$1' is missing, we cannot continue without it"
    fi
}

ensureDir ()
{
    if [ ! -d "$1" ]
    then
        mkdir "$1"
    fi
}

withRetry ()
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
            warn "Command '$@' failed. Attempt $n/$max:"
            sleep $delay;
        else
            fail "Command '$@' has failed after $n attempts."
        fi
    }
    done
}

logDuration ()
{
    # Save start-time, execute command and log elapsed time.
    local startTime=$SECONDS
    "$@" ||
    {
        warn "Command '$@' failed in '$((SECONDS - startTime))' seconds"
        return 1
    }

    info "Command '$@' succeeded in '$((SECONDS - startTime))' seconds"
    return 0
}
