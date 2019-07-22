#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Run the editmode and playmode unity tests.
# --------------------------------------------------------------------------------------------------

# Use the example dir for running tests as we need to have a unity-project.
EXAMPLE_DIR=".example"

# Verify that commands we depend on are present.
if doesntHaveCommand u3d
then
    fail "'u3d' required (more info: https://github.com/DragonBox/u3d)."
fi

# Verify that the example directory exists.
if [ ! -d "$EXAMPLE_DIR" ]
then
    fail "No example directory found at: '$EXAMPLE_DIR'."
fi

test()
{
    local platform="$1"
    local outputPath="$2"
    local logPath="$3"

    # Delete existing results.
    rm -f "$outputPath"
    rm -f "$logPath"

    info "Starting '$platform' tests (logfile: '$3')."

    # Execute tests with unity.
    (cd "$EXAMPLE_DIR" && u3d -- \
        -batchmode \
        -logfile "$logPath" \
        -nographics \
        -force-free \
        -runTests \
        -testPlatform "$platform" \
        -testResults "$outputPath") || true

    # Verify that a results-file was produced.
    if [ ! -f "$outputPath" ]
    then
        warn "Test run for '$platform' did not produce a result file at '$outputPath'."
        return 1
    fi

    info "Succesfully produced result for '$platform' at '$outputPath'."
    return 0
}

# Retry is here because Unity sometimes randomly fails during startup, we do NOT retry failed tests.
withRetry logDuration test "editmode" "$(pwd)/output/editmode.tests.xml" "$(pwd)/output/editmode.log"
withRetry logDuration test "playmode" "$(pwd)/output/playmode.tests.xml" "$(pwd)/output/playmode.log"

info "Succesfully ran tests."
exit 0
