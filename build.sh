#!/usr/bin/env bash

SRCPATH=$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

git rev-list --count HEAD > gitversion.txt

docker build $SRCPATH -f ./build/builder/Dockerfile --tag fileflows:build
docker run --rm --name FileFlowsBuild -v $SRCPATH:/src fileflows:build

