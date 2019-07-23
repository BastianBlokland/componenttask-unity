#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Open the Unity editor.
# --------------------------------------------------------------------------------------------------

# Verify dependencies.
if doesntHaveCommand u3d
then
    fail "'u3d' required (more info: https://github.com/DragonBox/u3d)."
fi

# Setup variables.
UNITY_PROJ_DIR="$1"
if [ ! -d "$UNITY_PROJ_DIR" ]
then
    fail "No directory found at: '$UNITY_PROJ_DIR', please provide the project directiory as arg1."
fi

info "Opening '$UNITY_PROJ_DIR' with 'u3d'."

# Launch unity with u3d.
(cd "$UNITY_PROJ_DIR" && u3d run)

info "Unity closed gracefully."
exit 0
