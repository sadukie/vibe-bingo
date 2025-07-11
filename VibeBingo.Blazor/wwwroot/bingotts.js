

window.getBingoVoices = function () {
    if ('speechSynthesis' in window) {
        let voices = window.speechSynthesis.getVoices();
        // If voices are not loaded yet, try to trigger loading
        if (!voices || voices.length === 0) {
            window.speechSynthesis.onvoiceschanged && window.speechSynthesis.onvoiceschanged();
            voices = window.speechSynthesis.getVoices();
        }
        return voices.map(v => ({
            name: v.name,
            lang: v.lang,
            default: v.default
        }));
    }
    return [];
};

window.registerVoicesChanged = function (dotNetObjRef) {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.onvoiceschanged = function () {
            dotNetObjRef.invokeMethodAsync('OnVoicesChanged');
        };
    }
};

window.speakBingoBall = function (text, voiceName) {
    if ('speechSynthesis' in window) {
        const utter = new window.SpeechSynthesisUtterance(text);
        if (voiceName) {
            const voices = window.speechSynthesis.getVoices();
            const match = voices.find(v => v.name === voiceName);
            if (match) utter.voice = match;
        }
        window.speechSynthesis.speak(utter);
    }
};
