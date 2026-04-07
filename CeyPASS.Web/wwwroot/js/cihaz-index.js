(function () {
    'use strict';

    /* ---- Silme onay modali ---- */
    var silOnayModalEl = document.getElementById('silOnayModal');
    var silOnayModalBody = document.getElementById('silOnayModalBody');
    var silOnayModalSilBtn = document.getElementById('silOnayModalSilBtn');

    if (silOnayModalEl && silOnayModalBody && silOnayModalSilBtn) {
        document.querySelectorAll('.btn-sil-onay').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var form = this.closest('form');
                var msg = this.getAttribute('data-msg') || 'Emin misiniz?';
                var btnText = this.getAttribute('data-btn-text') || 'Onayla';
                silOnayModalBody.textContent = msg;
                silOnayModalSilBtn.innerHTML = '<i class="bi bi-trash me-1"></i>' + btnText;
                var modal = new bootstrap.Modal(silOnayModalEl);
                silOnayModalSilBtn.onclick = function () { modal.hide(); if (form) form.submit(); };
                modal.show();
            });
        });
    }

    /* ---- QR Modal ---- */
    var qrModalEl = document.getElementById('qrModal');
    var qrImg = document.getElementById('qrImg');
    var qrCihazIsim = document.getElementById('qrCihazIsim');
    var qrYazdirBtn = document.getElementById('qrYazdirBtn');
    var qrError     = document.getElementById('qrError');
    var qrImgContainer = document.getElementById('qrImgContainer');
    var qrYazdirContainer = document.getElementById('qrYazdirContainer');
    var currentQrUrl = '';
    var currentIsim = '';

    /* Event delegation: DataTables'in yeniden render ettigi butonlar da calisir */
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-qr-modal');
        if (!btn) return;

        var id = btn.getAttribute('data-id');
        var isim = btn.getAttribute('data-isim');
        currentIsim = isim;
        currentQrUrl = '/Cihaz/QrKod/' + id;

        // Reset states before loading
        qrImgContainer.classList.add('d-none');
        qrYazdirContainer.classList.add('d-none');
        qrError.classList.add('d-none');

        qrImg.src = currentQrUrl;
        
        qrImg.onerror = function() {
            qrError.classList.remove('d-none');
        };
        qrImg.onload = function() {
            qrImgContainer.classList.remove('d-none');
            qrYazdirContainer.classList.remove('d-none');
        };

        qrCihazIsim.innerText = isim;

        var modal = new bootstrap.Modal(qrModalEl);
        modal.show();
    });

    /* ---- Yazdir / PDF ---- */
    if (qrYazdirBtn) {
        qrYazdirBtn.addEventListener('click', function () {
            if (!currentQrUrl) return;

            var imgUrl = window.location.origin + currentQrUrl;
            var content = '<!DOCTYPE html><html>'
                + '<head><title>QR Kod - ' + currentIsim + '</title>'
                + '<style>'
                + '* { margin:0; padding:0; box-sizing:border-box; }'
                + 'body { display:flex; flex-direction:column; align-items:center; justify-content:center;'
                + '       min-height:100vh; background:#fff; font-family:Arial,sans-serif; padding:10px; }'
                + 'h2 { font-size:18px; font-weight:bold; color:#222; margin-bottom:14px; text-align:center; }'
                + '.qrimg { width:88vmin; height:88vmin; display:block; }'
                + 'p { margin-top:10px; font-size:12px; color:#666; text-align:center; }'
                + '</style></head>'
                + '<body>'
                + '<h2>' + currentIsim + '</h2>'
                + '<img class="qrimg" src="' + imgUrl + '" />'
                + '<p>Bu QR kodu, güvenli konum bilgisi tarar.</p>'
                + '<p>Sahtecilik tespiti halinde yaptırıma tabiidir.</p>'
                + '<' + 'script>window.onload=function(){window.print();}<' + '/script>'
                + '</body></html>';

            var printWin = window.open('', '_blank', 'width=700,height=800');
            printWin.document.write(content);
            printWin.document.close();
        });
    }
})();
