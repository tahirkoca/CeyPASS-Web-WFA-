/** Personel formunda zorunlu alan (*) göstergelerini checkbox'lara göre günceller. */
(function () {
    function setMarker(id, on) {
        var el = document.getElementById(id);
        if (!el) return;
        el.classList.toggle("d-none", !on);
    }

    function syncPersonelRequiredMarkers() {
        var firma = document.getElementById("firmaPersoneli");
        var taseron = document.getElementById("taseronCalisanMi");
        var ziyaretci = document.getElementById("ziyaretciMi");
        var arac = document.getElementById("aracKartiMi");
        var yemek = document.getElementById("yemekHakkiVar");

        var firmaOn = !!(firma && firma.checked);
        var taseronOn = !!(taseron && taseron.checked);
        var ziyaretciOn = !!(ziyaretci && ziyaretci.checked);
        var aracOn = !!(arac && arac.checked);
        var yemekOn = !!(yemek && yemek.checked);

        setMarker("req-tc", firmaOn || taseronOn);
        setMarker("req-kart", ziyaretciOn || aracOn);
        setMarker("req-yemek", yemekOn);
    }

    function bind() {
        ["firmaPersoneli", "taseronCalisanMi", "ziyaretciMi", "aracKartiMi", "yemekHakkiVar"].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.addEventListener("change", syncPersonelRequiredMarkers);
        });
        syncPersonelRequiredMarkers();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", bind);
    } else {
        bind();
    }

    window.syncPersonelRequiredMarkers = syncPersonelRequiredMarkers;
})();
