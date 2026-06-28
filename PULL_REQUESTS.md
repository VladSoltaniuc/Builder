# Pull Request Best Practices

## Branch naming
```
feat/short-description
fix/what-you-fixed
chore/what-you-cleaned
```

## Commit messages
- Use lowercase, present tense: `add login page`, `fix null check on order update`
- Prefix matches the branch: `feat:`, `fix:`, `chore:`
- One thing per commit - don't bundle unrelated changes
- Bad: `stuff`, `wip`, `fix`, `asdfgh`
- Good: `fix missing auth header on profile request`

## Before you open a PR
- [ ] Tested locally - backend builds, frontend loads, feature actually works
- [ ] No leftover `console.log`, commented-out code, or debug changes
- [ ] Self-review your own diff before requesting review - read it like a stranger would

## PR size
**One PR = one thing.** If you can't summarize it in a sentence, it's too big.  
Split it. Small PRs get reviewed faster and break less.

## Title and description
- Title: `add email verification on registration` (not `updates` or `fix stuff`)
- Description: one line on **why**, one line on **how to test it**

## Rules
- **Never push directly to `main` or `dev`** - always open a PR
- **Never merge your own PR** - someone else reviews it first
- **Resolve your own conflicts** - don't open a PR that can't merge cleanly
- **One approval minimum** before merging

## When your PR is reviewed
- Respond to every comment, even if just `done` or `disagree because X`
- Don't push force to a PR branch after review starts - add new commits instead

## Red flags that will get your PR rejected
- PR touches 10 different files for "one small change"
- No description, title is vague
- Mixing refactoring with feature changes in the same PR
- Breaks existing tests, if a test is outdated update it in the same PR - reviewers will also read your test changes, so don't be a sneaky beaver and add true == true :,) 

## For juniors
- If you made it this far in the documentation, you are tough as hell. Keep your head up and don't let all those "do this" "do that" requirems get you down, sooner or later you will understand the utility of them all and how they save you so much time, so for now just trust me.
- Don't forget to have fun, no point in this if you ain't enjoying your time
- Contact me for anything you might need, I'm here for you