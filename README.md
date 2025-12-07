# SmartDocs – IA para Consulta de Documentos

SmartDocs é uma aplicação full-stack que permite fazer perguntas sobre documentos utilizando IA + embeddings + similaridade vetorial.
O backend em .NET faz o chunking/indexação dos documentos e o frontend em Angular oferece uma interface simples para interação com o chat.

🚀 Tecnologias Utilizadas
- Backend (.NET 8)
- ASP.NET Core Web API
- Ollama
- Embeddings + Similaridade de vetores
- Armazenamento em IVectorStore (custom)
- Chunking automático por documento

🧠 Fluxo do Chat Inteligente

- O usuário seleciona um documento
- O Angular carrega todos os chunks
- O usuário envia uma pergunta
- O backend:
- Gera embedding da pergunta
- Busca chunks semânticos relevantes
- Monta o prompt com contexto
- Gera resposta da IA
