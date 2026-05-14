#!/usr/bin/env bash
# Deploy the local WebGL build to the gh-pages branch.
#
# Prereqs:
#   - Builds/WebGL/ must contain a fresh build (build via Unity Editor first).
#   - main branch must already be pushed to the origin remote.
#   - The gh-pages branch must exist on the remote (created automatically the
#     first time this script runs).
#
# Usage:
#   ./scripts/deploy.sh

set -euo pipefail

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$PROJECT_ROOT/Builds/WebGL"
WORKTREE_DIR="$PROJECT_ROOT/.gh-pages-worktree"
BRANCH="gh-pages"

cd "$PROJECT_ROOT"

if [[ ! -d "$BUILD_DIR" ]] || [[ -z "$(ls -A "$BUILD_DIR")" ]]; then
  echo "Error: $BUILD_DIR is missing or empty. Build the WebGL target in Unity first." >&2
  exit 1
fi

if ! git rev-parse --git-dir > /dev/null 2>&1; then
  echo "Error: not a git repository." >&2
  exit 1
fi

if ! git ls-remote --exit-code --heads origin "$BRANCH" > /dev/null 2>&1; then
  echo "Remote branch '$BRANCH' does not exist — creating it as an orphan."
  git worktree add --detach "$WORKTREE_DIR"
  pushd "$WORKTREE_DIR" > /dev/null
  git checkout --orphan "$BRANCH"
  git rm -rf . 2>/dev/null || true
  popd > /dev/null
else
  git worktree add "$WORKTREE_DIR" "$BRANCH"
fi

trap 'git worktree remove --force "$WORKTREE_DIR" > /dev/null 2>&1 || true' EXIT

rsync -a --delete --exclude='.git' "$BUILD_DIR/" "$WORKTREE_DIR/"

# Stamp sw.js with the current git short SHA so every deploy invalidates installed
# PWA caches automatically. Without this, the manually-bumped CACHE_VERSION easily
# drifts out of sync with the build, leaving users stuck on stale .wasm/.data forever.
GIT_SHA=$(git rev-parse --short HEAD)
if [[ -f "$WORKTREE_DIR/sw.js" ]]; then
  sed -i.bak -E "s/zozoblast-v[a-z0-9]+/zozoblast-${GIT_SHA}/" "$WORKTREE_DIR/sw.js"
  rm "$WORKTREE_DIR/sw.js.bak"
fi

# Tell GitHub Pages not to run Jekyll (otherwise it ignores files starting with _).
touch "$WORKTREE_DIR/.nojekyll"

pushd "$WORKTREE_DIR" > /dev/null
git add -A
if git diff --cached --quiet; then
  echo "No changes to deploy."
else
  COMMIT_MSG="Deploy WebGL build $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  git commit -m "$COMMIT_MSG"
  git push origin "$BRANCH"
  echo "Deployed: $COMMIT_MSG"
fi
popd > /dev/null
