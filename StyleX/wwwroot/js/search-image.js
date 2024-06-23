import { env, AutoTokenizer, CLIPTextModelWithProjection } from 'https://cdn.jsdelivr.net/npm/@xenova/transformers@2.7.0';

const EMBED_DIM = 512;
env.allowLocalModels = false;

async function getCachedFile(url) {
    let cache;
    try {
        cache = await caches.open('image-database');
        const cachedResponse = await cache.match(url);
        if (cachedResponse) {
            return await cachedResponse.arrayBuffer();
        }
    } catch (e) {
        console.warn('Unable to open cache', e);
    }

    // No cache, or cache failed to open. Fetch the file.
    const response = await fetch(url);
    const buffer = await response.arrayBuffer();

    if (cache) {
        try {
            // NOTE: We use `new Response(buffer, ...)` instead of `response.clone()` to handle LFS files
            await cache.put(url, new Response(buffer, {
                headers: response.headers,
            }));
        } catch (e) {
            console.warn('Unable to cache file', e);
        }
    }

    return buffer;
}

function cosineSimilarity(query_embeds, database_embeds) {
    const numDB = database_embeds.length / EMBED_DIM;
    const similarityScores = new Array(numDB);

    for (let i = 0; i < numDB; ++i) {
        const startOffset = i * EMBED_DIM;
        const dbVector = database_embeds.slice(startOffset, startOffset + EMBED_DIM);

        let dotProduct = 0;
        let normEmbeds = 0;
        let normDB = 0;

        for (let j = 0; j < EMBED_DIM; ++j) {
            const embedValue = query_embeds[j];
            const dbValue = dbVector[j];

            dotProduct += embedValue * dbValue;
            normEmbeds += embedValue * embedValue;
            normDB += dbValue * dbValue;
        }

        similarityScores[i] = dotProduct / (Math.sqrt(normEmbeds) * Math.sqrt(normDB));
    }

    return similarityScores;
}



async function getCachedJSON(url) {
    let buffer = await getCachedFile(url);

    let decoder = new TextDecoder('utf-8');
    let jsonData = decoder.decode(buffer);

    return JSON.parse(jsonData);
}



class ApplicationSingleton {
    static model_id = 'Xenova/clip-vit-base-patch16';
    static BASE_URL = 'https://huggingface.co/datasets/Xenova/semantic-image-search-assets/resolve/main/';

    static tokenizer = null;
    static text_model = null;
    static metadata = null;
    static embeddings = null;

    static async getInstance(progress_callback = null) {
        // Load tokenizer and text model
        if (this.tokenizer === null) {
            this.tokenizer = AutoTokenizer.from_pretrained(this.model_id, { progress_callback });
        }
        if (this.text_model === null) {
            this.text_model = CLIPTextModelWithProjection.from_pretrained(this.model_id, { progress_callback });
        }
        if (this.metadata === null) {
            this.metadata = getCachedJSON(this.BASE_URL + 'image-embeddings.json');
        }
        if (this.embeddings === null) {
            this.embeddings = new Promise(
                (resolve, reject) => {
                    getCachedFile(this.BASE_URL + 'image-embeddings_25k-512-32bit.bin')
                        .then((buffer) => {
                            resolve(new Float32Array(buffer));
                        })
                        .catch(reject);
                }
            )
        }

        return Promise.all([this.tokenizer, this.text_model, this.metadata, this.embeddings]);
    }
}




export async function SearchImages(query = '', pageSize = 20, page = 1) {
    const [tokenizer, text_model, metadata, embeddings] = await ApplicationSingleton.getInstance();

    // Run tokenization
    const text_inputs = tokenizer(query, { padding: true, truncation: true });
    
    // Compute embeddings
    const { text_embeds } = await text_model(text_inputs);
    
    // Compute similarity scores
    const scores = cosineSimilarity(text_embeds.data, embeddings);
    
    // Make a copy of the metadata
    let output = metadata.slice(0);
    
    // Add scores to output
    for (let i = 0; i < metadata.length; ++i) {
        output[i].score = scores[i];
    }
    
    // Sort by score
    output.sort((a, b) => b.score - a.score);
    
    // Get results
    const resultsIgnore  = (page-1)*pageSize;
    output = output.slice(resultsIgnore, pageSize);
    return output;
}

