#!/bin/bash
set -e
source ./.ci/utils.sh

# --------------------------------------------------------------------------------------------------
# Build api doc site using 'docfx'.
# --------------------------------------------------------------------------------------------------

# Verify dependencies.
if doesntHaveCommand docfx
then
    fail "'docfx' required (more info: https://dotnet.github.io/docfx/)."
fi

DOCS_DIR=".docfx"
if [ ! -d "$DOCS_DIR" ]
then
    fail "Docs directory '$DOCS_DIR' is missing"
fi

# Gather metadata from our source.
(cd "$DOCS_DIR" && docfx metadata)

# Build documentation site.
(cd "$DOCS_DIR" && docfx build)

info "Finished building documentation."
exit 0
