# Regras de negócio

Este documento serve para registrar as decisões e regras de negócio seguidas na modelagem do projeto. A ideia é que qualquer pessoa possa ler e entender os porquês do comportamento do projeto.

## Visão geral

O sistema gira ao redor de três entidades principais: o **paciente**, a **vacina** e a **vacinação**. O paciente é a pessoa que possui a carteira de vacinação, a vacina é literalmente a vacina e a vacinação é o registro que determinado paciente tomou determinada dose de uma determinada vacina.

O paciente atua como raiz de agregação porque é ela que possui a carteira de vacinação. Por isso, sempre que eu preciso registrar uma vacinação, passamos pelo paciente. Usando o paciente como referência, a gente sabe se a dose faz sentido. Ex.: não faz sentido a pessoa tomar a 3a dose de uma vacina para a qual ela não tomou nem a 1a dose.

## Paciente

- O paciente tem nome e identificador único
- O nome é obrigatório e não pode ser vazio / conter apenas espaços
- Ao excluir um paciente, são excluídos a carteira de vacinação e os registros de vacinação associados a ela.

## Vacina

- A vacine tem nome e identificador único
- Cada vacina tem um total de doses previstas. Por exemplo, a vacina contra a febre amarela é dose única e, portanto, o total de doses é `1`.

## Vacinação

Esse é o registro que "liga" as duas entidades (vacina e paciente). Ela representa a administração de uma dose de imunobiológicos no paciente. Por exemplo: o paciente X recebeu a segunda dose da vacina contra COVID-19. Esta informação é um registro da entidade "Vacinação".

### Regras de validação

1. A vacina e a pessoa precisam existir
2. A dose tem que estar dentro da faixa - por exemplo, se a vacina tem 3 doses, são aceitas apenas as doses 1, 2 e 3. A quarta dose é rejeitada.
3. Não é permitido repetir uma dose
4. As doses seguem ordens
5. A data de aplicação não pode ser uma data futura

### Exclusão de registros de vacinação

É possível excluir um registro de vacinação específico da carteira de vacinação do paciente. No entanto, cada paciente só pode apagar o seu próprio registro e um ID errado é tratado como não encontrado para evitar vazamento de informações.

### Exclusão lógica

Para favorecer a auditabilidade, eu optei por seguir com a exclusão lógica: informações continuam no banco de dados, mas possuem a sinalização de que estão apagadas, permitindo a recuperação dos dados, caso necessário.
