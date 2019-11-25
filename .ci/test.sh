#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Run the editmode and playmode Unity tests.
# --------------------------------------------------------------------------------------------------

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
    (cd "$EXAMPLE_DIR" && unity \
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


# Use the example dir for running tests as we need to have a unity-project.
EXAMPLE_DIR=".example"

# Verify that the example directory exists.
if [ ! -d "$EXAMPLE_DIR" ]
then
    fail "No example directory found at: '$EXAMPLE_DIR'."
fi

# Run tests.
withRetry logDuration test "editmode" "$(pwd)/.output/editmode.tests.xml" "$(pwd)/.output/editmode.log"
withRetry logDuration test "playmode" "$(pwd)/.output/playmode.tests.xml" "$(pwd)/.output/playmode.log"

info "Succesfully ran tests."
exit 0
