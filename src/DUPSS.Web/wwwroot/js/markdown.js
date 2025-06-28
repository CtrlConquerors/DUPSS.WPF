const simpleMDEInstances = {};

function initializeSimpleMDE(selector) {
    const element = document.querySelector(selector);
    if (!element) {
        console.error(`Element ${selector} not found`);
        return;
    }

    // Clean up existing instance if any
    if (simpleMDEInstances[selector]) {
        simpleMDEInstances[selector].toTextArea();
        delete simpleMDEInstances[selector];
    }

    // Initialize new SimpleMDE
    const simpleMDE = new SimpleMDE({
        element: element,
        spellChecker: false,
        toolbar: [
            "bold", "italic", "strikethrough", "|",
            "heading-1", "heading-2", "heading-3", "|",
            "code", "quote", "unordered-list", "ordered-list", "|",
            "link", "image", "table", "|",
            "preview", "side-by-side", "fullscreen"
        ]
    });

    simpleMDEInstances[selector] = simpleMDE;
    console.log(`SimpleMDE initialized for ${selector}`);
}

function setSimpleMDEContent(selector, content) {
    const simpleMDE = simpleMDEInstances[selector];
    if (simpleMDE) {
        simpleMDE.value(content || "");
        console.log(`Set content for ${selector}: ${content}`);
    } else {
        console.error(`SimpleMDE instance not found for ${selector}`);
    }
}

function getSimpleMDEContent(selector) {
    const simpleMDE = simpleMDEInstances[selector];
    if (simpleMDE) {
        const content = simpleMDE.value();
        console.log(`Get content for ${selector}: ${content}`);
        return content;
    } else {
        console.error(`SimpleMDE instance not found for ${selector}`);
        return "";
    }
}