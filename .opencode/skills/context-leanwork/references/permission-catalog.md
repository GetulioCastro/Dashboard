# Catálogo de Permissões — `opencode.json`

Receitas de `allow` / `ask` / `deny` por ecossistema, usadas pela skill `context-leanwork` ao gerar a seção `permission` do `opencode.json`.

## OpenCode vs Claude Code

Este catálogo foi originalmente derivado do plugin Leanwork SDD para Claude Code. A versão atual foi adaptada para a semântica nativa do **OpenCode**, que difere do Claude Code em pontos importantes:

- **Sintaxe:** OpenCode usa `"pattern": "ação"` como chave-valor de objeto. Claude Code usava `Tool(especificador)` — sintaxe que **não existe** no OpenCode.
- **Precedência:** OpenCode avalia todas as regras e a **última** que casar vence. Claude Code usava primeira regra que casar, com precedência `deny → ask → allow`.
- **Curingas:** OpenCode usa `*` (zero+ caracteres) e `?` (um caractere). Não há suporte a `**` como glob recursivo nos padrões de ferramenta (o padrão `**` pode funcionar em `read` como caminho, mas não está documentado oficialmente — ver nota na seção Deny universal).

Exemplos neste catálogo seguem a sintaxe OpenCode. Referências históricas ao Claude Code são mantidas apenas quando necessárias para proveniência.

---

## Como as regras funcionam

- **Formato:** `"padrão": "ação"` — onde padrão é um glob e ação é `allow`, `ask` ou `deny`.
- **Precedência:** a **última** regra que casar com a entrada decide. A ordem das regras importa.
- **Escopos se somam:** as permissões de `opencode.json` (projeto), `openrc.json` (raio do usuário, diretórios de projeto) e `~/.config/opencode/config.json` (usuário) são combinadas. Um `deny` em qualquer escopo não pode ser afrouxado por um `allow` em outro.
- **`deny` não aceita exceção.** Não existe "negar tudo menos X" — para isso, use `allow` específico com `defaultMode` restritivo.
- **Ferramentas:** `read`, `edit`, `bash`, `glob`, `grep`, `webfetch`, `websearch`, `task`, `skill`, `external_directory`. Cada uma avalia padrões diferentes (caminhos para `read`/`edit`, texto de comando para `bash`, etc.).

### Precedência: última regra que casar vence

No OpenCode, **todas** as regras são avaliadas contra a entrada. A regra que vence é a **última** que casar — independentemente de ser `allow`, `ask` ou `deny`.

```jsonc
"bash": {
  "*": "ask",              // 1ª regra: tudo pede confirmação
  "rm *": "deny",          // 2ª regra: rm é bloqueado
  "git status*": "allow"   // 3ª regra: git status é liberado
}
```

- `rm -rf /tmp` → casa com `"*"` (ask), casa com `"rm *"` (deny) → **deny** vence (última que casou)
- `git status` → casa com `"*"` (ask), casa com `"git status*"` (allow) → **allow** vence
- `dotnet build` → casa com `"*"` (ask) → **ask** vence

**Consequência prática:** regras mais específicas devem vir **depois** da regra genérica. Colocar uma regra restritiva antes de uma permissiva faz a permissiva vencer — o oposto do que se espera.

### Curingas

- `*` — casa com zero ou mais de qualquer caractere
- `?` — casa com exatamente um caractere
- Todos os outros caracteres casam literalmente

**Exemplos para `bash`:**

| Padrão | Casamento |
|--------|-----------|
| `"dotnet build*"` | `dotnet build`, `dotnet build --no-restore`, `dotnet build.sln` |
| `"git status*"` | `git status`, `git status --porcelain` |
| `"git push * --force*"` | `git push origin main --force`, `git push --force` |
| `"git push * -f*"` | `git push origin main -f`, `git push -f` |
| `"rm -rf*"` | `rm -rf /tmp`, `rm -rf .` |
| `"git reset --hard*"` | `git reset --hard HEAD~1` |
| `"git clean -fdx*"` | `git clean -fdx`, `git clean -fdx -n` |

**Nota sobre `--force-with-lease`:** `"git push * --force*"` casa com `git push --force-with-lease` (o curinga `*` depois de `--force` aceita `-with-lease`). Se quiser permitir `--force-with-lease`, adicione deny explícito para as variantes sem lease e não inclua deny para a forma com lease.

### Sombra de prefixo entre baldes

Com a semântica de última regra que casar, a "sombra de prefixo" funciona de forma diferente do modelo anterior:

```jsonc
// CORRETO: regra ampla primeiro, específica depois
"bash": {
  "*": "ask",
  "npx tsc*": "ask",    // redundante — já coberto pelo "*"
  "npx jest*": "allow"  // vence para comandos npx jest
}

// INCORRETO: específica antes da ampla
"bash": {
  "npx jest*": "allow",  // esta regra NÃO vence
  "*": "ask"             // esta regra vence (é a última que casou)
}
```

**Como conferir uma receita:** para cada entrada do `allow`, verificar se há alguma regra mais ampla **depois** dela que case com o mesmo comando. Se houver, a regra ampla vence e a entrada do `allow` é código morto.

**Diferença fundamental:** no modelo anterior (primeira regra que casar), uma regra `ask` larga antes de uma `allow` específica tornava a `allow` inalcançável. No modelo OpenCode (última que casar), o problema inverte: uma regra `ask` larga **depois** de uma `allow` específica mata a `allow`. A solução é sempre manter a regra genérica no início e as específicas no final.

## Onde cada coisa mora

| Arquivo | Escopo | Versionado | Uso |
|---|---|---|---|
| `opencode.json` (raiz) | Projeto | **Sim** — viaja com o repo | Política do time |
| `openrc.json` | Projeto, pessoal | Não (geralmente fora do versionamento) | Overrides de máquina do dev |
| `~/.config/opencode/config.json` | Usuário | Não | Preferências pessoais em todos os projetos |

A skill escreve **apenas** o `opencode.json` na raiz. Preferências individuais são do dev, não do projeto.

---

## Deny universal — vale para qualquer stack

Esta lista é o núcleo de segurança e entra em **todo** projeto, independente de tecnologia. São arquivos que carregam segredo em texto puro.

```jsonc
"read": {
  "**": "allow",
  // Variáveis de ambiente
  "*.env": "deny",
  "*.env.*": "deny",

  // Chaves e certificados
  "*.pem": "deny",
  "*.key": "deny",
  "*.pfx": "deny",
  "*.p12": "deny",
  "*id_rsa*": "deny",
  "*id_ed25519*": "deny",

  // Credenciais de cloud e infra
  "*.aws/credentials": "deny",
  "*.kube/config": "deny",
  "*.kubeconfig": "deny",
  "*terraform.tfstate": "deny",
  "*terraform.tfstate.*": "deny",
  "*.tfvars": "deny",

  // Tokens de registry
  "*.npmrc": "deny",
  "*.pypirc": "deny",
  "*.docker/config.json": "deny"
},

"bash": {
  "*": "ask",

  // Comandos destrutivos
  "rm -rf*": "deny",
  "git reset --hard*": "deny",
  "git clean -fdx*": "deny",

  // Force push — variantes que cobrem forma longa, curta,
  // e a flag depois do remote.
  "git push * --force*": "deny",
  "git push * -f*": "deny",
  "git push --force*": "deny",
  "git push -f*": "deny"
}
```

**Sobre `terraform.tfstate`:** costuma ser esquecido e é dos piores — guarda senha de banco, chave de API e token em texto puro. Sempre incluir quando o projeto tem Terraform.

**Sobre `.npmrc` e `.pypirc`:** carregam token de publicação em registry. Ler é o primeiro passo para vazar.

**Nota sobre `**` em padrões de `read`:** a documentação oficial do OpenCode mostra `"*.env": "deny"` como padrão default. O uso de `**` (como `"**/.env"`) pode funcionar para caminhos aninhados (ex: `config/.env`), mas não está documentado explicitamente. A abordagem conservadora é usar `"*.env"` (que nega `.env` na raiz) e adicionar `"**/.env"` como cobertura adicional para subdiretórios — se `**` não for suportado como glob recursivo, a regra extra é ignorada sem efeito colateral. O `opencode.json` do DashboardSB atualmente usa `"**/.env"` e funciona; manter ambas as formas é a opção mais segura.

---

## .NET

```jsonc
// Dentro de "bash":
"dotnet build*": "allow",
"dotnet test*": "allow",
"dotnet run*": "allow",
"dotnet restore*": "allow",
"dotnet format*": "allow",
"dotnet ef migrations list*": "allow",
"dotnet ef migrations script*": "allow",

"dotnet add package*": "ask",
"dotnet ef migrations add*": "ask",
"dotnet ef database update*": "ask",
"dotnet nuget push*": "ask",

// Dentro de "read":
"**/appsettings.Production.json": "deny",
"**/appsettings.*.Production.json": "deny",
"**/secrets.json": "deny",

// Dentro de "bash":
"dotnet ef database drop*": "deny"
```

**Notas de calibragem:**
- `dotnet ef migrations script` e `list` são leitura — seguros no `allow`. `add` e `database update` alteram estado — vão para `ask`.
- `dotnet ef database drop` é destrutivo e irreversível: `deny`, não `ask`.
- `secrets.json` é o User Secrets do .NET, que vive fora do repo mas é lido pela aplicação.
- `dotnet add package` no `ask` protege contra supply chain — instalação silenciosa de pacote é vetor real.

## Node.js / TypeScript

```jsonc
// Dentro de "bash":
"npm run build*": "allow",
"npm run test*": "allow",
"npm run lint*": "allow",
"npm run dev*": "allow",
"npm ci*": "allow",

"npm install*": "ask",
"npm i*": "ask",
"npx*": "ask",

"npm publish*": "deny",

// Dentro de "read":
"*.npmrc": "deny"
```

**Adaptar ao gerenciador detectado pelo lockfile:** `pnpm-lock.yaml` → trocar `npm` por `pnpm`; `yarn.lock` → `yarn`; `bun.lockb` → `bun`. Usar o gerenciador errado gera regra que nunca casa.

**Sobre `npx` no `ask`:** `npx` executa pacote arbitrário baixado na hora — sempre passa por confirmação. Não adiantar exceções nomeadas (`npx tsc`, `npx prisma`) no `allow`: o `ask` mais largo vence pela precedência e a regra específica vira código morto. Para um `npx` frequente, declarar o comando como script no `package.json` e liberar `npm run <script>*`.

## Python

```jsonc
// Dentro de "bash":
"pytest*": "allow",
"ruff*": "allow",
"black*": "allow",
"mypy*": "allow",
"python -m pytest*": "allow",

"pip install*": "ask",
"poetry add*": "ask",
"uv add*": "ask",
"alembic upgrade*": "ask",
"python manage.py migrate*": "ask",

"twine upload*": "deny",
"python manage.py flush*": "deny",

// Dentro de "read":
"*.pypirc": "deny"
```

Adaptar ao gerenciador detectado: `poetry.lock` → poetry; `uv.lock` → uv; `Pipfile.lock` → pipenv.

## Go

```jsonc
// Dentro de "bash":
"go build*": "allow",
"go test*": "allow",
"go run*": "allow",
"go vet*": "allow",
"go fmt*": "allow",
"golangci-lint run*": "allow",

"go get*": "ask",
"go install*": "ask"
```

## Java / Kotlin

```jsonc
// Dentro de "bash":
"./gradlew build*": "allow",
"./gradlew test*": "allow",
"mvn compile*": "allow",
"mvn test*": "allow",

"./gradlew publish*": "ask",
"mvn deploy*": "ask",
"./gradlew flywayMigrate*": "ask",

// Dentro de "read":
"*.jks": "deny",
"*.keystore": "deny"
```

## Rust

```jsonc
// Dentro de "bash":
"cargo build*": "allow",
"cargo test*": "allow",
"cargo run*": "allow",
"cargo clippy*": "allow",
"cargo fmt*": "allow",

"cargo add*": "ask",
"cargo install*": "ask",

"cargo publish*": "deny"
```

## PHP

```jsonc
// Dentro de "bash":
"php artisan test*": "allow",
"./vendor/bin/phpunit*": "allow",
"./vendor/bin/pint*": "allow",

"composer require*": "ask",
"composer update*": "ask",
"php artisan migrate*": "ask",

"php artisan migrate:fresh*": "deny",
"php artisan db:wipe*": "deny"
```

## Ruby

```jsonc
// Dentro de "bash":
"bundle exec rspec*": "allow",
"bundle exec rubocop*": "allow",
"bin/rails test*": "allow",

"bundle add*": "ask",
"bin/rails db:migrate*": "ask",

"bin/rails db:drop*": "deny",
"gem push*": "deny"
```

---

## Git — transversal a qualquer stack

```jsonc
// Dentro de "bash":
"git status*": "allow",
"git diff*": "allow",
"git log*": "allow",
"git show*": "allow",
"git branch*": "allow",
"git add*": "allow",
"git stash*": "allow",

"git commit*": "ask",
"git push*": "ask",
"git merge*": "ask",
"git rebase*": "ask",
"gh pr create*": "ask",
"gh pr merge*": "ask",

"git push * --force*": "deny",
"git push * -f*": "deny",
"git push --force*": "deny",
"git push -f*": "deny",
"git reset --hard*": "deny",
"git clean -fdx*": "deny"
```

**Sobre `git commit` no `ask`:** decisão de estilo. Time que usa o agente para commits frequentes pode mover para `allow` — o commit é local e reversível. Perguntar ao usuário antes de gravar.

**Por que quatro regras para negar force push.** Proteção contra reescrita de histórico compartilhado. No OpenCode, com last-match-wins, são necessárias variantes para cobrir diferentes posições da flag:

| Comando | Regra que pega |
|---|---|
| `git push --force`, `git push --force origin main` | `"git push --force*"` |
| `git push -f origin main` | `"git push -f*"` |
| `git push origin main --force` | `"git push * --force*"` |
| `git push origin main -f` | `"git push * -f*"` |

**`--force-with-lease` e a regra `"git push * --force*"`:** o curinga `*` depois de `--force` aceita `-with-lease`. Logo, `git push --force-with-lease` **é negado** por esta regra. Se quiser permitir `--force-with-lease` (que é seguro — preserva verificação remota), remova a regra `"git push * --force*"` e adicione variantes mais específicas:

```jsonc
"git push --force ": "deny",   // casa com "git push --force origin main"
"git push --force*": "allow",  // casa com "git push --force-with-lease"
"git push * --force ": "deny",
"git push * --force*": "allow"
```

Ou, mais simples, se o time não se importa em negar `--force-with-lease`:
```jsonc
"git push * --force*": "deny",
"git push --force*": "deny"
```

**Buraco que permanece:** `git push origin +main` faz force push via refspec e não casa com nenhuma das quatro regras. Negar `"git push * +*"` cobre o caso, ao custo de falsos positivos em refspecs legítimos com `+`. Decisão do time; o padrão da skill é não incluir.

**Por que `git add` e `git stash` vão para `allow`:** são operações locais e inteiramente reversíveis. `git add` apenas prepara staging (pode ser desfeito com `git reset HEAD <file>`). `git stash` salva trabalho em progresso (revertido com `git stash pop`). Liberar essas operações elimina fricção desnecessária sem risco.

**`.gitignore` não é uma camada de segurança.** O `.gitignore` controla apenas o que `git add` e `git status` consideram — não impede o agente de ler ou editar arquivos ignorados. Um `.env` listado em `.gitignore` continua acessível pela ferramenta `read`. As regras `deny` na seção `read` do `opencode.json` é que protegem de fato: `.gitignore` é conveniência de workflow, não barreira de segurança.

---

## Docker e infraestrutura local

```jsonc
// Dentro de "bash":
"docker compose up*": "allow",
"docker compose stop*": "allow",
"docker compose start*": "allow",
"docker compose logs*": "allow",
"docker ps*": "allow",

"docker compose down*": "ask",
"docker build*": "ask",
"docker push*": "ask",

"docker system prune*": "deny",
"docker volume rm*": "deny",
"docker volume prune*": "deny",
"kubectl delete*": "deny",
"terraform apply*": "deny",
"terraform destroy*": "deny"
```

**Sobre `docker compose down` no `ask`:** `down -v` remove os volumes nomeados declarados na seção `volumes` **e** os anônimos ligados aos containers — banco de desenvolvimento, seed, fixtures. Sem a flag, `down` só remove containers e redes, que o `up` recria: o risco está inteiramente no `-v`. `stop` cobre o caso cotidiano de parar o ambiente sem tocar em nada persistente, e `start` traz de volta.

**Por que não negar apenas o `-v`.** Seria a regra mais precisa, e não é escrevível com segurança: o `deny` não aceita exceção, e a flag aparece em qualquer posição — `down --remove-orphans -v`, `-f compose.yml down -v`, `--volumes` por extenso. Cobrir as variantes exigiria uma dezena de regras, e a primeira que faltasse seria a que passa. Estreitar o `allow` resolve em uma linha. Ver "Sombra de prefixo entre baldes".

**Sobre `docker volume rm` e `docker volume prune` no `deny`:** rota mais curta para a mesma perda que o `down -v`. `prune` sem flag remove os volumes anônimos não usados; com `-a`, também os nomeados. `docker system prune` já cobre a forma `--volumes` pelo casamento de prefixo.

**Buraco que permanece:** `docker compose up -V` (`--renew-anon-volumes`) descarta o conteúdo dos volumes anônimos ao recriar os containers. Volume nomeado não é afetado — por isso o `up` continua no `allow`. Projeto que guarde estado em volume anônimo deve mover `up` para o `ask`.

**Sobre Terraform no `deny`:** `apply` mexe em infraestrutura real e custa dinheiro. Mesmo `ask` é arriscado — a confirmação vira reflexo. Time que faz IaC com agente deveria mover para `ask` conscientemente, nunca deixar em `allow`.

---

## Princípios de calibragem

**Menor privilégio por padrão.** Preferir prefixo específico a tool inteira. `"dotnet build*"` em vez de `"dotnet*"`, e nunca `"*"` sozinho no `allow`.

**A pergunta que decide o balde:**

| Pergunta | Balde |
|---|---|
| É leitura ou operação local reversível? | `allow` |
| Altera estado externo, mas é recuperável? | `ask` |
| É irreversível, destrutivo, ou expõe segredo? | `deny` |

**Fadiga de confirmação é risco de segurança.** Lista `ask` grande demais treina o dev a aprovar no automático, o que anula a proteção. Melhor um `allow` generoso no que é seguro e um `deny` firme no que é perigoso, com `ask` reservado ao que realmente merece pausa.

**`deny` é a única camada que não pode ser afrouxada.** Como as listas de todos os escopos se somam e `deny` vence sempre (mesmo com last-match-wins, o deny tem precedência sobre ask e allow quando ambos casam — ver documentação oficial), é onde vale investir rigor: um `deny` no projeto protege todo mundo do time, independente das configurações pessoais de cada dev.

**Não bloquear o que o pipeline SDD precisa.** O agente lê `docs/**` o tempo todo (PRDs, planos, ADRs). Nunca colocar `docs/` em `deny`.

**Ordem das regras no `bash`:** manter a regra genérica `"*": "ask"` **no início** do objeto, e as regras específicas (allow, deny) **depois**. Com last-match-wins, as regras específicas no final vencem a genérica no início — que é exatamente o comportamento desejado.

**Ordem das regras no `read`:** o padrão `"**": "allow"` deve ser o **primeiro**. Regras de deny para arquivos sensíveis vêm depois e vencem (last-match-wins). A regra `"**": "allow"` genérica é o catch-all para leitura; as exceções de segurança ficam nas posições seguintes.

---

## Referências

- [OpenCode — Permissions](https://opencode.ai/docs/permissions/) — documentação oficial das permissões
- [OpenCode — Config](https://opencode.ai/docs/config/) — estrutura do `opencode.json`
