#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Utility to launch the example project in the correct Unity version.
# --------------------------------------------------------------------------------------------------

EXAMPLE_DIR=".example"

# Verify that commands we depend on are present.
if doesntHaveCommand u3d
then
    fail "'u3d' required, more info: https://github.com/DragonBox/u3d"
fi

# Verify that the example directory exists.
if [ ! -d "$EXAMPLE_DIR" ]
then
    fail "No example directory found at: '$EXAMPLE_DIR'"
fi

# Launch unity with u3d.
(cd "$EXAMPLE_DIR" && u3d run)
