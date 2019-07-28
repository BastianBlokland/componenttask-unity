.PHONY: install-dependencies test edit-example docs
default: test

# --------------------------------------------------------------------------------------------------
# MakeFile used as a convient way for executing development utlitities.
# --------------------------------------------------------------------------------------------------

install-dependencies:
	./.ci/install-dependencies.sh

test:
	./.ci/test.sh

edit-example:
	./.ci/open-unity.sh '.example'

docs:
	./.ci/docs-build.sh
