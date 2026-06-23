---
marp: true
paginate: true
transition: fade
# PechaKucha: 6 slides, 20s auto-advance. Do not change the count.
auto-advance: 20
---

<!-- slide 1 -->
# Who's my person?
<!-- 20s -->

**A Myanmar household with a broken AC**

- They call a shop, get told "we'll come tomorrow"
- Tomorrow comes — nobody shows up
- They call again — no answer, no update
- Hours wasted, still no cool air

---

<!-- slide 2 -->
# Their problem

**No way to know when help is coming**

- Myanmar is phone-dominant — but phone tag doesn't work
- No email dependency — most people don't use email for services
- No transparency — is the repair guy coming or not?
- They just want: book → know when → done

---

<!-- slide 3 -->
# What I built

**ServiceCity — AC service booking platform**

- 🔧 Book AC repair, maintenance, installation, gas refill
- 📋 Get a unique reference number (SC-XXXXXXXX)
- 📍 Track booking status with visual timeline
- 🔔 In-app notifications when status changes
- 📊 Admin dashboard — accept, decline, schedule
- 📱 Mobile-first responsive (Bootstrap 5)

---

<!-- slide 4 -->
# How I built it

**Stack:** ASP.NET Core MVC (.NET 10) + EF Core + PostgreSQL + Bootstrap 5

- **MCP:** `context7` — live docs. `playwright` — UI screenshots. `codebase-memory-mcp` — code graph & trace. `microsoft-docs` — .NET API reference
- **Skill:** `servicecity-dev` — conventions, migration workflow, Docker, security rules
- **Agent:** `gsd-code-reviewer` — adversarial review, 34 GSD agents for planning & audit
- **Git:** commit-as-you-build on `main`, Docker Compose for local testing

---

<!-- slide 5 -->
# Why it matters

**No phone tag. No uncertainty.**

- For users: book in 2 minutes, know exactly when help comes
- For shop: one dashboard, no missed calls, no lost bookings
- For Myanmar: built for how people actually communicate — phones, not email
- Simple enough for v1, structured enough to grow

---

<!-- slide 6 -->
# Done checklist
- [x] repo public — `github.com/thetzinsoe/ServiceCity`
- [x] MCP + skill + agent used — 4 MCPs, 1 skill, 34 agents
- [x] report.md in team repo — `ch-3/thetzinsoe/report.md`
- [x] .mcp.json, .claude/skills/, .claude/agents/ all present
- [ ] 3 GitHub stars ⭐⭐⭐ (ask teammates!)
