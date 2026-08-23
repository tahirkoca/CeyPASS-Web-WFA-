(function (global) {
    function isValid(tc) {
        if (!tc || !String(tc).trim()) return false;
        var t = String(tc).trim();
        return t.length === 11 && /^\d+$/.test(t);
    }

    function looksMasked(text) {
        return !!text && String(text).indexOf("*") >= 0;
    }

    function mask(tc) {
        if (!tc || !String(tc).trim()) return "";
        var t = String(tc).trim();
        if (t.length <= 1) return t;
        return t.charAt(0) + "*".repeat(t.length - 1);
    }

    function requireValid(tc) {
        var t = (tc || "").trim();
        if (!t) throw new Error("T.C. Kimlik No giriniz.");
        if (looksMasked(t) || !isValid(t))
            throw new Error("T.C. Kimlik No 11 haneli olmalıdır.");
        return t;
    }

    function resolveForSave(displayText, tamTc) {
        var shown = (displayText || "").trim();
        if (looksMasked(shown)) return requireValid(tamTc);
        return requireValid(shown);
    }

    global.TcKimlik = {
        isValid: isValid,
        looksMasked: looksMasked,
        mask: mask,
        requireValid: requireValid,
        resolveForSave: resolveForSave
    };
})(window);
