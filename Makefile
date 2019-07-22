.PHONY: install-dependencies test edit-example
default: test

# --------------------------------------------------------------------------------------------------
# MakeFile used as a convient way for executing development utlitities.
# --------------------------------------------------------------------------------------------------

install-dependencies:
	./.ci/install-dependencies.sh

test:
	./.ci/test.sh

edit-example:
	./.ci/edit-example.sh
