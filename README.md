# BanGround-Unity

These rules must be enforced after open-source.

## Gitflow
1. No direct push to `master` or `release` branch.
All work must be done on a separate branch and a pull request should be created to merge the changes to `master` branch.
A pull request from `master` to `release` should be created for each build.

*Exception*: Small fixes that do not cause behavioral change are allowed to be commited directly to `master`.

2. Commit messages, pull requests, and branch names should clearly describe the purpose.

3. Massive scene changes should be done on a separate branch.
An issue should be created to describe the purpose of the changes and to notify other developers that the scene is being modified to avoid merge conflicts.

4. All pull requests must be reviewed and approved by at least one other developer before merging.
