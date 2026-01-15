---
description: Pull request creation workflow with comprehensive validation, user confirmation, and platform-agnostic support for GitHub and Azure DevOps, featuring automated change analysis, conflict detection, and graceful fallback mechanisms.
auto_execution_mode: 1
---

# Pull Request Creation Workflow

As a Senior DevOps Engineer with expertise in Git workflows, CI/CD automation, and platform integrations, facilitate safe and transparent pull request creation with comprehensive validation and user control. This workflow combines automated repository analysis, change statistics generation, conflict detection, and platform-specific PR creation (GitHub/Azure DevOps) to ensure safe, well-documented pull requests. The process emphasizes explicit user confirmation, transparent change summaries, and graceful fallback to CLI tools when needed, maintaining the highest standards for code collaboration and deployment safety.

## Input Parameters

**Optional Parameters:**
`--platform=PLATFORM`: Target platform - "github" or "azuredevops" (default: auto-detect from remote URL)
- `--title="PR Title"`: Custom PR title (default: auto-generated from commits or branch name)
- `--description="Description"`: Custom PR description (default: auto-generated from template or commits)
- `--base=BRANCH`: Target base branch (default: auto-detected main/master)
- `--draft`: Create as draft PR (default: false)
- `--reviewers=user1,user2`: Comma-separated list of reviewers (default: none)
- `--no-push`: Dry-run mode, preview only without pushing changes (default: false)
- `--work-items=123,456`: Azure DevOps work item IDs to link (default: none, Azure DevOps only)
- `--project=NAME`: Azure DevOps project name (default: auto-detect, Azure DevOps only)
- `--repository=NAME`: Repository name/ID (default: auto-detect)

### Parameter Validation
- - Validate platform parameter is "github" or "azuredevops" if provided, otherwise auto-detect from remote URL
- Verify current branch is not the base branch
- Validate base branch exists in remote repository
- Check authentication status for specified/detected platform (gh CLI for GitHub, az CLI for Azure DevOps)
- Validate reviewer usernames/emails if provided
 Ensure work item IDs exist if provided (Azure DevOps only)
- Validate repository and project names if provided

## Output

**Print to Console:**
- Change summary with repository, branch, and statistics information
- Confirmation prompt with detailed actions to be performed
- PR creation success message with PR number and URL
- **Do not save PR summary as file** (console output only)

### Structured Output Format (console output only)

**Change Summary Display:**
```
===========================================================
                    CHANGE SUMMARY
===========================================================

Repository: owner/repo
Current Branch: feature/new-feature -> main

Committed Changes:
   - Commits: 3
   - Files Added: 2
   - Files Modified: 5
   - Files Deleted: 0
   - Lines Added: +150
   - Lines Deleted: -25

Recent Commits:
feat: Add new feature
fix: Resolve bug in component
docs: Update README

===========================================================
```

**Confirmation Prompt:**
- List all actions: stage, commit, push, create PR
- Show PR details: title, draft status, reviewers
- Require explicit "YES" confirmation (case-insensitive)

**Success Output:**
- PR number/ID and title
- PR URL for direct access
- Status, draft state, author information
- Change statistics summary
**Console output only** (do not save as file)

- Print the following:
  - List of rules applied to the workflow in bulleted format
  - Evaluation Scores in tabular format with average score
  - Evaluation summary (less than 100 words)
  **Do not save as file.** (Console output only)

## Core Principles

### User Control
- Always obtain explicit confirmation before pushing changes
- Require "YES" (case-insensitive) for final confirmation
- Support dry-run mode with `--no-push` flag for preview
- Allow cancellation at any point in the workflow

### Transparency
- Provide clear summary of changes and actions (under 200 words)
- Display comprehensive change statistics before proceeding
- Show all git operations that will be performed
- Present PR content preview before creation

### Safety First
- Validate repository state before making changes
- Detect and warn about potential merge conflicts
- Check if branch is behind base branch
- Verify authentication before attempting operations
- Never auto-commit or auto-push without user approval

### Platform Agnostic
- Auto-detect platform from remote URL (GitHub/Azure DevOps)
- Use platform-specific MCP tools for primary operations
- Support both GitHub and Azure DevOps workflows
- Adapt output format to platform conventions

### Graceful Fallback
- Fall back to CLI commands if MCP tools fail
- Provide clear error messages with actionable solutions
- Handle authentication failures gracefully
- Support offline analysis with `--no-push` mode

## Execution Flow

### Phase 1: Repository Context Validation
**Repository State Analysis:**
1. Verify git repository initialization: `git rev-parse --git-dir`
2. Check remote configuration: `git config --get remote.origin.url`
3. Detect platform from remote URL (github.com vs dev.azure.com)
4. Extract repository owner/organization and name from URL
5. Get current branch: `git branch --show-current`
6. Auto-detect base branch: `git symbolic-ref refs/remotes/origin/HEAD`
7. Validate current branch is not base branch

**Authentication Verification:**
- For GitHub: Verify `gh auth status`
- For Azure DevOps: Verify `az account show` and `az devops configure --list`
- Display authentication errors with login instructions

### Phase 2: Change Analysis
**File and Commit Analysis:**
1. Scan staged changes: `git diff --cached --name-status`
2. Scan unstaged changes: `git diff --name-status`
3. List untracked files: `git ls-files --others --exclude-standard`
4. Analyze commits against base: `git log --oneline $BASE..$HEAD`
5. Count commits: `git rev-list --count $BASE..$HEAD`
6. Get file statistics: `git diff --stat $BASE..$HEAD`
7. Calculate line changes: `git diff --shortstat $BASE..$HEAD`

**Change Categorization:**
- Added files (A status)
- Modified files (M status)
- Deleted files (D status)
- Renamed files (R status)
- Total additions and deletions

### Phase 3: PR Content Generation
**Title Generation Strategy:**
1. Use custom title if provided via `--title` parameter
2. If single commit: Extract and use commit message as title
3. If multiple commits: Convert branch name to readable format
   - Remove prefixes: `feature/`, `bugfix/`, `hotfix/`, `chore/`
   - Replace hyphens/underscores with spaces
   - Capitalize each word

**Description Generation:**
1. Check for PR template in repository:
   - GitHub: `.github/PULL_REQUEST_TEMPLATE.md`
   - Azure DevOps: `.azuredevops/pull_request_template.md` or `docs/pull_request_template.md`
2. If template exists: Use as base structure
3. If no template: Auto-generate with:
   - Summary section from commit messages (limit 10 commits)
   - Files changed section (limit 20 files)
   - Basic test plan checklist
   - Claude Code attribution

### Phase 4: Conflict Detection
**Pre-merge Validation:**
1. Fetch latest base branch: `git fetch origin $BASE_BRANCH`
2. Check if behind base: `git rev-list --count HEAD..origin/$BASE`
3. Warn user if behind (suggest rebase or merge)
4. Detect potential conflicts: `git merge-tree $(git merge-base HEAD origin/$BASE) HEAD origin/$BASE`
5. Parse conflict markers and affected files
6. Display conflict warning with option to proceed or cancel

### Phase 5: User Confirmation
**Comprehensive Confirmation Display:**
- Repository information (platform, owner/org, repo name)
- Branch information (source -> target)
- Change statistics (commits, files, lines)
- Actions to be performed (numbered list)
- PR details (title, description preview, draft status, reviewers)

**Confirmation Requirements:**
- Display clear prompt: "Proceed with PR creation? (YES/NO):"
- Accept only "YES", "yes", or "Yes" to proceed
- Any other input cancels the operation
- In dry-run mode (`--no-push`), skip confirmation and show preview only

### Phase 6: Stage, Commit, and Push
**Pre-push Preparation:**
1. If uncommitted changes exist:
   - Stage all changes: `git add -A`
   - Create commit with message format:
     ```
     <PR title or custom message>

     Generated with Claude Code

     Co-Authored-By: Claude <noreply@anthropic.com>
     ```
2. Check if branch exists remotely: `git ls-remote --exit-code --heads origin $BRANCH`
3. Push to remote:
   - New branch: `git push -u origin $BRANCH`
   - Existing branch: `git push origin $BRANCH`
4. Verify push success before proceeding to PR creation

### Phase 7: Pull Request Creation (Platform-Specific)

**GitHub PR Creation:**
1. **Primary Method**: Use `mcp__github__create_pull_request`
   - Parameters: owner, repo, title, head, base, body, draft
   - Extract owner and repo from remote URL
   - Set head as current branch, base as target branch
2. **Fallback Method**: Use `gh pr create` CLI command
   - Command: `gh pr create --title "..." --body "..." --base main --head feature-branch`
   - Add `--draft` flag if draft PR requested
   - Parse output for PR number and URL
3. **Add Reviewers** (if specified):
   - Use `gh pr edit <number> --add-reviewer "user1,user2"`

**Azure DevOps PR Creation:**
1. **Primary Method**: Use `mcp__azure-devops__repo_create_pull_request`
   - Parameters: repositoryId, sourceRefName, targetRefName, title, description, isDraft
   - Format ref names: `refs/heads/branch-name`
   - Link work items via `workItems` parameter if provided
2. **Fallback Method**: Use `az repos pr create` CLI command
   - Command: `az repos pr create --title "..." --description "..." --source-branch feature --target-branch main`
   - Add `--draft true` if draft PR requested
   - Add `--work-items 123 456` if work items specified
   - Parse output for PR ID and URL
3. **Add Reviewers** (if specified):
   - Use `mcp__azure-devops__repo_update_pull_request_reviewers`

### Phase 8: Post-Creation Actions
**Verification and Reporting:**
1. Fetch PR details for verification:
   - GitHub: `mcp__github__get_pull_request` or `gh pr view <number>`
   - Azure DevOps: `mcp__azure-devops__repo_get_pull_request_by_id`
2. Verify PR creation success
3. Display comprehensive success summary with:
   - PR number/ID and title
   - PR URL (clickable link)
   - Status (Open/Draft)
   - Author information
   - Change statistics
4. Provide next steps guidance to user

### Technical Implementation

**Git Commands by Category:**

1. **Repository Information:**
   - `git status --porcelain` - Check file changes
   - `git diff --stat` - Get change statistics
   - `git branch --show-current` - Get current branch
   - `git config --get remote.origin.url` - Get repository URL
   - `git symbolic-ref refs/remotes/origin/HEAD` - Get default branch

2. **Change Analysis:**
   - `git diff --name-status $BASE..$HEAD` - List changed files with status
   - `git diff --shortstat $BASE..$HEAD` - Get addition/deletion counts
   - `git log --oneline $BASE..$HEAD` - Get commit history
   - `git rev-list --count $BASE..$HEAD` - Count commits

3. **Conflict Detection:**
   - `git fetch origin $BASE_BRANCH` - Update remote references
   - `git rev-list --count HEAD..origin/$BASE` - Check if behind base
   - `git merge-tree $(git merge-base HEAD origin/$BASE) HEAD origin/$BASE` - Check conflicts

4. **Staging and Pushing:**
   - `git add -A` - Stage all changes
   - `git commit -m "message"` - Create commit
   - `git push -u origin $BRANCH` - Push new branch
   - `git push origin $BRANCH` - Push existing branch
   - `git ls-remote --exit-code --heads origin $BRANCH` - Check remote branch exists

**MCP Tools by Platform:**

**GitHub Tools:**
- `mcp__github__create_pull_request` - Primary PR creation
- `mcp__github__get_pull_request` - Fetch PR details
- `mcp__github__search_users` - Find reviewers (optional)

**Azure DevOps Tools:**
- `mcp__azure-devops__repo_create_pull_request` - Primary PR creation
- `mcp__azure-devops__repo_get_pull_request_by_id` - Fetch PR details
- `mcp__azure-devops__repo_update_pull_request_reviewers` - Add reviewers

**CLI Fallback Commands:**
- GitHub: `gh pr create`, `gh pr edit`, `gh pr view`
- Azure DevOps: `az repos pr create`, `az repos pr reviewer add`

## Guardrails
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/code-anti-patterns.md`: Avoid god objects, circular deps, magic constants
- `rules/dry-principle-guidelines.md`: Single source of truth; delta updates
- `rules/iterative-development-guide.md`: Strict phased workflow
- `rules/language-agnostic-standards.md`: KISS, YAGNI, size limits, clear naming
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy, code fences
- `rules/performance-best-practices.md`: Optimize after measurement
- `rules/security-standards-owasp.md`: OWASP Top 10 alignment
- `rules/software-architecture-patterns.md`: Pattern selection, boundaries

### Extended Standards (apply based on task domain)
- `rules/code-documentation-standards.md`: Comment WHY, not WHAT
- `rules/react-development-standards.md`: React component patterns
- `rules/typescript-styleguide.md`: TypeScript typing & consistency
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA validation
- `rules/backend-development-standards.md`: Service/controller patterns
- `rules/frontend-development-standards.md`: Frontend patterns
- `rules/ui-ux-design-standards.md`: Layout/interaction standards
- `rules/database-standards.md`: Schema/migration standards
- `rules/stored-procedure-standards.md`: Stored procedure patterns
- `rules/dotnet-architecture-standards.md`: .NET architecture patterns

**Selection**: Apply only standards matching task domain. Most specific overrides general.

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the PR creation process to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### PR Creation Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Safety Compliance | [PASS/FAIL] | MUST PASS | User confirmation obtained; no auto-push; proper error handling |
| 2 | Validation Completeness | [0-100%] | >=80% | Repo state validated; auth checks; conflict detection |
| 3 | Change Documentation | [0-100%] | >=80% | Accurate statistics; clear summaries |
| 4 | Content Quality | [0-100%] | >=80% | Well-formatted PR title and description; template usage |
| 5 | Platform Integration | [0-100%] | >=80% | Platform detection; MCP tool usage; CLI fallback |
| 6 | User Experience | [0-100%] | >=80% | Clear prompts; informative output; logical workflow |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | User confirmation prompt present; no auto-push detected | Auto-push: BLOCKED |
| 2 | Repo state check, auth validation, conflict detection performed | Missing check: -15%/check |
| 3 | Change statistics accurate; summary reflects actual diff | Inaccurate stat: -10%/instance |
| 4 | PR title follows convention; description uses template | Bad format: -10%/issue |
| 5 | Platform detected; MCP used if available; CLI fallback works | Integration failure: -15% |
| 6 | Clear prompts at each step; output informative | Unclear step: -10%/step |

**Overall Score**: [Weighted Average]%

**Evaluation Summary** (Top 3 Weaknesses):
1. **[Lowest Dimension]** ([X]%): [Specific reason]
2. **[2nd Lowest]** ([X]%): [Specific reason]
3. **[3rd Lowest]** ([X]%): [Specific reason]

**Critical Failures**: [List any MUST gates that failed, or "None"]

### Self-Healing Protocol

**Retry Limits:** 1 patch attempt per failing dimension | Mode: Edit-in-place only

**Patchable:**
| Dimension | Patch Action |
|-----------|--------------|
| Validation Completeness | Add missing repo checks |
| Change Documentation | Fix statistics accuracy |
| Content Quality | Reformat PR title/description |
| Platform Integration | Retry with CLI fallback |
| User Experience | Improve prompt clarity |

**Non-Patchable (Skip):** Safety Compliance (requires user confirmation), auth failures, source missing, patch >30% of section, >=3 failing dimensions

**Retry Flow:**
```
IF output file missing: ABORT (no retry)
READ existing output
FOR each failing dimension:
  IF patchable AND patch < 30%: EDIT in-place; RE-EVALUATE once
  ELSE: REPORT "[MANUAL FIX] {dimension}"
IF >=3 manual fixes needed: ABORT all retries
```

---
