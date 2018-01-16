# IDCardReadService
二代身份证读卡Windows服务  
注意：目前不支持读取卡内头像文件

## 调用方式
```javascript
  function getidcard(){
     $.ajax({
         async:false,
         type:"get",
         dataType:"json",
         url:"http://localhost:27812/read",
         success:function(data){
             var rs=eval(data);
             $("#fullname").val(rs.Name);
             $("#sex_code").val(rs.Sex);
             $("#sex_name").val(rs.SexName);
             $("#nation_code").val(rs.Nation);
             $("#nation_name").val(rs.NationName);
             $("#birthday").val(rs.Born);
             $("#address").val(rs.Address);
             $("#id_code").val(rs.IDCardNo);
             $("#grant_dept").val(rs.GrantDept);
             $("#valid_begin").val(rs.ValidBegin);
             $("#valid_end").val(rs.ValidEnd);
         },
         error: function (jqXHR, textStatus, errorThrown) {
             if(jqXHR.responseText!=null&&jqXHR.responseText!=""&&jqXHR.responseText!=undefined){
                 alert("读取身份证出错："+jqXHR.responseText);
             }else{
                 alert("请先确认身份证扫描器是否连接正常");
             }            
        }
     })
  }
```
